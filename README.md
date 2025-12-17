# ProObjLogLite
Lite version of [ProObjLog](https://github.com/RobertFlexx/ProObjLog)

## Why does this project exist?
Essentially, I liked ProObjLogger, but I found its codebase to be too complex. That is why I got rid of unnecessary object-orientation and also added quality of life features such as flag-based arguments.

## How do I use it?
It's really similar to the regular ProObjLog, you spawn it as a process and it does the logging for you. 

### Example
```bash
ProObjLogLite -d ImportantLogs -l INFO -m 'This is a message!'
```

In this example, we call the program with flag arguments. The flag ```-d``` is the directory where the log is to be stored. 

```-l``` determines the level, this could be, for example, WARN, INFO, FATAL etc.

```-m``` represents the message of the log, this could really be anything you like and depends on what you want to log exactly.

There is also the ```-n``` flag, this ensures that the log will only be saved in the file and not printed to the console.

Logs look like this:
```bash
[06:07:39 PM / Wednesday, December 17, 2025]
Level: INFO
Message: This is a message
```

## Why should I use it?
If you need a simple logger that can easily be integrated into existing apps, this might be for you.

Because it is its own program, you can use it from other languages, not just C#. 

You could, for example, write a function to call ProObjLogLite with parameters. In Python, this could look like this:

```python
import subprocess
def log(l: str, m: str) -> None:
    subprocess.Popen(["ProObjLogLite", "-d", "ErrorLogs", "-l", l, "-m", m])

log("FATAL", "Error while executing function Foo.")
```
