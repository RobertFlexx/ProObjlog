#!/usr/bin/env perl
use strict;
use warnings;

# Try likely build locations (newest first)
my @candidates = (
    "./bin/Debug/net9.0/ProObjLog",
    "./bin/Release/net9.0/ProObjLog",
    "./bin/Debug/net8.0/ProObjLog",
    "./bin/Release/net8.0/ProObjLog",
    "./ProObjLog",          # in case you run from publish root
    "./out/ProObjLog",      # dotnet publish -o out
);

my $exe;
for my $p (@candidates) {
    if (-x $p) {
        $exe = $p;
        last;
    }
}

die "ProObjLog executable not found. Build it first with 'dotnet build'.\n"
    unless defined $exe;

# args
my $level = uc(shift @ARGV // 'INFO');
my $text  = join(' ', @ARGV);

# call the C# logger
my @cmd = ($exe, $level, $text);
system @cmd;

if ($? != 0) {
    my $code = $? >> 8;
    die "ProObjLog exited with code $code\n";
}
