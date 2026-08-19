#!/usr/bin/env python3
"""
ProObjLogLite world-style integration and stress tester.

This tool simulates realistic production-style logging workloads across multiple
services and failure modes, exercising a broad set of ProObjLogLite features.
"""

from __future__ import annotations

import argparse
import json
import random
import shutil
import string
import subprocess
import sys
import tempfile
import threading
import time
from dataclasses import dataclass, field
from pathlib import Path
from typing import Dict, List, Sequence


LEVELS = ["TRACE", "DEBUG", "INFO", "WARN", "ERROR", "FATAL"]
SERVICES = ["auth", "payments", "orders", "shipping", "api-gateway", "worker"]
OUTCOMES = ["allow", "deny", "fallback", "retry", "defer"]


@dataclass
class RunStats:
    passed: int = 0
    failed: int = 0
    skipped: int = 0
    return_codes: Dict[int, int] = field(default_factory=dict)

    def record(self, rc: int, expected: Sequence[int]) -> None:
        self.return_codes[rc] = self.return_codes.get(rc, 0) + 1
        if rc in expected:
            self.passed += 1
        else:
            self.failed += 1


def rand_id(prefix: str, n: int = 8) -> str:
    tail = "".join(random.choice(string.ascii_lowercase + string.digits) for _ in range(n))
    return f"{prefix}-{tail}"


def run_log(bin_path: str, args: List[str], stdin_text: str | None = None) -> subprocess.CompletedProcess[str]:
    return subprocess.run(
        [bin_path] + args,
        input=stdin_text,
        text=True,
        capture_output=True,
        check=False,
    )


def assert_ok(proc: subprocess.CompletedProcess[str], label: str, expected: Sequence[int] = (0,)) -> None:
    if proc.returncode not in expected:
        raise RuntimeError(
            f"{label} failed (rc={proc.returncode})\n"
            f"STDOUT:\n{proc.stdout}\nSTDERR:\n{proc.stderr}"
        )


def base_flags(work_dir: Path) -> List[str]:
    return [
        "--dir",
        str(work_dir / "logs"),
        "--source",
        random.choice(SERVICES),
        "--event-id",
        rand_id("evt", 6),
        "--cid",
        rand_id("cid", 10),
        "--context",
        f"region={random.choice(['us-east', 'us-west', 'eu-central'])}",
        "--context",
        f"node={rand_id('node', 4)}",
    ]


def scenario_bootstrap(bin_path: str, work_dir: Path) -> None:
    assert_ok(run_log(bin_path, ["--version"]), "version")
    assert_ok(run_log(bin_path, ["--list-levels"]), "list-levels")
    assert_ok(run_log(bin_path, ["--init-config"]), "init-config first")
    proc = run_log(bin_path, ["--init-config"])
    assert_ok(proc, "init-config second")


def scenario_timer_and_decision(bin_path: str, work_dir: Path) -> None:
    timer_name = rand_id("deploy", 5)
    assert_ok(run_log(bin_path, ["--timer", timer_name, "--timer-start", "--utc"]), "timer-start")
    time.sleep(0.05)
    args = base_flags(work_dir) + [
        "--timer",
        timer_name,
        "--timer-stop",
        "--decision",
        "cache_strategy",
        "--outcome",
        random.choice(OUTCOMES),
        "--type",
        "decision",
        "--message",
        "Decision recorded after timer",
    ]
    assert_ok(run_log(bin_path, args), "timer-stop+decision")


def scenario_logic_and_assert(bin_path: str, work_dir: Path) -> None:
    args_ok = base_flags(work_dir) + [
        "--type",
        "logic",
        "--context",
        "status=ok",
        "--assert",
        "status == ok",
        "--message",
        "Health gate pass",
    ]
    assert_ok(run_log(bin_path, args_ok), "logic assert pass")

    args_fail = base_flags(work_dir) + [
        "--type",
        "logic",
        "--context",
        "status=fail",
        "--assert",
        "status == ok",
        "--message",
        "Health gate fail triggers fatal level",
    ]
    assert_ok(run_log(bin_path, args_fail), "logic assert fail still logs")


def scenario_sampling_and_conditional(bin_path: str, work_dir: Path) -> None:
    for _ in range(10):
        args = base_flags(work_dir) + [
            "--sample-rate",
            "0.35",
            "--when",
            "env == prod",
            "--context",
            f"env={random.choice(['prod', 'staging'])}",
            "--message",
            "Conditionally sampled event",
        ]
        proc = run_log(bin_path, args)
        assert_ok(proc, "sample+when")


def scenario_redaction_and_chain(bin_path: str, work_dir: Path) -> None:
    file_path = work_dir / "secure" / "audit.log"
    args = base_flags(work_dir) + [
        "--type",
        "audit",
        "--chain-hash",
        "--file",
        str(file_path),
        "--redact-keys",
        "password,token,secret",
        "--context",
        "password=supersecret",
        "--exception",
        "token=abc123 refresh failed",
        "--message",
        "user login failed password=supersecret",
    ]
    assert_ok(run_log(bin_path, args), "redaction+chain")


def scenario_metrics_and_rotation(bin_path: str, work_dir: Path) -> None:
    log_file = work_dir / "metrics" / "runtime.log"
    for _ in range(35):
        args = base_flags(work_dir) + [
            "--type",
            "metric",
            "--metric",
            f"latency_ms={random.randint(10, 1200)}",
            "--metric",
            f"cpu_pct={random.randint(1, 99)}",
            "--metric",
            f"mem_mb={random.randint(80, 1500)}",
            "--max-size",
            "16KB",
            "--max-files",
            "4",
            "--file",
            str(log_file),
            "--message",
            "Runtime metric sample",
        ]
        assert_ok(run_log(bin_path, args), "metrics+rotation")


def scenario_smart_and_profiles(bin_path: str, work_dir: Path) -> None:
    args_ci = base_flags(work_dir) + [
        "--profile",
        "ci",
        "--smart",
        "--message",
        "payment failed due to timeout in worker",
        "--json",
    ]
    proc = run_log(bin_path, args_ci)
    assert_ok(proc, "profile+smart")
    if proc.stdout.strip().startswith("{"):
        json.loads(proc.stdout.splitlines()[0])

    args_prod = base_flags(work_dir) + [
        "--profile",
        "prod",
        "--message",
        "warn: slow response from downstream",
    ]
    assert_ok(run_log(bin_path, args_prod), "profile prod")


def scenario_stdin_stdout_dryrun(bin_path: str, work_dir: Path) -> None:
    stdin_proc = run_log(
        bin_path,
        base_flags(work_dir)
        + [
            "--json",
            "--message",
            "-",
            "--stdout-only",
        ],
        stdin_text="pipeline payload for stdin mode",
    )
    assert_ok(stdin_proc, "stdin/stdout-only")

    dry_proc = run_log(
        bin_path,
        base_flags(work_dir)
        + [
            "--dry-run",
            "--message",
            "Preview-only deployment plan",
        ],
    )
    assert_ok(dry_proc, "dry-run")


def scenario_concurrency(bin_path: str, work_dir: Path, workers: int, iterations: int) -> None:
    errors: List[str] = []
    lock = threading.Lock()

    def worker(wid: int) -> None:
        local_rand = random.Random(wid * 997 + int(time.time()))
        for i in range(iterations):
            try:
                msg = f"worker={wid} seq={i} level noise check"
                level = local_rand.choice(LEVELS)
                args = [
                    "--dir",
                    str(work_dir / "concurrency"),
                    "--source",
                    f"stress-worker-{wid}",
                    "--level",
                    level,
                    "--json",
                    "--message",
                    msg,
                    "--count",
                    str(local_rand.randint(1, 5)),
                    "--sample-rate",
                    "0.9",
                    "--chain-hash",
                    "--max-size",
                    "64KB",
                    "--max-files",
                    "3",
                ]
                proc = run_log(bin_path, args)
                if proc.returncode != 0:
                    with lock:
                        errors.append(f"worker={wid} i={i} rc={proc.returncode}")
            except Exception as exc:
                with lock:
                    errors.append(f"worker={wid} i={i} {exc!r}")

    threads = [threading.Thread(target=worker, args=(i,), daemon=True) for i in range(workers)]
    for t in threads:
        t.start()
    for t in threads:
        t.join()

    if errors:
        raise RuntimeError("Concurrency scenario failures: " + ", ".join(errors[:10]))


def parse_args() -> argparse.Namespace:
    p = argparse.ArgumentParser(description="ProObjLogLite serious world-style tester")
    p.add_argument("--bin", default="ProObjLogLite", help="Path to ProObjLogLite binary")
    p.add_argument("--workers", type=int, default=6, help="Concurrent workers")
    p.add_argument("--iterations", type=int, default=30, help="Iterations per worker")
    p.add_argument("--seed", type=int, default=None, help="Random seed")
    p.add_argument("--keep", action="store_true", help="Keep temporary output directory")
    return p.parse_args()


def resolve_bin(candidate: str) -> str:
    if shutil.which(candidate) or Path(candidate).is_file():
        return candidate

    candidate_path = Path(candidate)
    if candidate_path.is_absolute():
        return candidate

    fallback = Path(__file__).resolve().parent.parent / ".publish" / "ProObjLogLite"
    if fallback.is_file():
        print(f"[world-test] '{candidate}' not found; using built binary {fallback}")
        return str(fallback)

    return candidate


def main() -> int:
    args = parse_args()
    args.bin = resolve_bin(args.bin)
    if args.seed is not None:
        random.seed(args.seed)

    stats = RunStats()
    tmp_ctx = tempfile.TemporaryDirectory(prefix="proobjlog_world_test_")
    work_dir = Path(tmp_ctx.name)

    scenarios = [
        ("bootstrap", lambda: scenario_bootstrap(args.bin, work_dir)),
        ("timer+decision", lambda: scenario_timer_and_decision(args.bin, work_dir)),
        ("logic+assert", lambda: scenario_logic_and_assert(args.bin, work_dir)),
        ("sampling+conditional", lambda: scenario_sampling_and_conditional(args.bin, work_dir)),
        ("redaction+chain", lambda: scenario_redaction_and_chain(args.bin, work_dir)),
        ("metrics+rotation", lambda: scenario_metrics_and_rotation(args.bin, work_dir)),
        ("smart+profiles", lambda: scenario_smart_and_profiles(args.bin, work_dir)),
        ("stdin+stdout+dryrun", lambda: scenario_stdin_stdout_dryrun(args.bin, work_dir)),
        ("concurrency", lambda: scenario_concurrency(args.bin, work_dir, args.workers, args.iterations)),
    ]

    print(f"[world-test] work_dir={work_dir}")
    for name, fn in scenarios:
        started = time.perf_counter()
        try:
            fn()
            elapsed = (time.perf_counter() - started) * 1000
            print(f"[PASS] {name:<24} {elapsed:8.1f} ms")
            stats.record(0, (0,))
        except Exception as exc:
            elapsed = (time.perf_counter() - started) * 1000
            print(f"[FAIL] {name:<24} {elapsed:8.1f} ms  {exc}")
            stats.record(99, (0,))

    print("\n=== Result Summary ===")
    print(f"passed: {stats.passed}")
    print(f"failed: {stats.failed}")
    print(f"return_codes: {stats.return_codes}")

    if args.keep:
        print(f"Kept artifacts at: {work_dir}")
    else:
        tmp_ctx.cleanup()

    return 0 if stats.failed == 0 else 1


if __name__ == "__main__":
    sys.exit(main())
