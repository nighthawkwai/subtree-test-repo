# Process Management

This namespace contains code that will allow process sandboxing. Eventually we will envolve this to more win 32 specific process libraries.

## Getting started with JobObjects

`JobObject` [class](xref:Microsoft.Services.Core.Process.JobObject) lets you sandbox processes via a kernel construct called [JobObject]
(https://msdn.microsoft.com/en-us/library/windows/desktop/ms684161(v=vs.85).aspx).

When a process is added to a job object, its children are also added to the same JobObject automatically. This lets you manage process trees.

### 1. Sandbox a process to not use more than 20% CPU.

```csharp
JobObject mgr = new JobObject();
mgr.SetJobLimits(new JobObjectLimit()
    .SetCpuLimit(2000));
mgr.AddProcess(Process.GetCurrentProcess());

while(true){
//This infinite loop will not peg the CPU
}
```

### 2. Sandbox a process to not use more than 40 MB  of virtual memory

```csharp
JobObject mgr = new JobObject();
mgr.SetJobLimits(new JobObjectLimit()
    .SetProcessCommitMemory(40*1024*1024));
mgr.AddProcess(Process.GetCurrentProcess());

while(true){
//This infinite loop will not peg the CPU
}
```

### 3. Launch a process reliably and run it and clean up all its children.

```csharp
JobObject mgr = new JobObject();
//Create a process suspended
//leaving it as an exercise to the reader.
//...
var process = CreateProcessSuspended("myExe.exe");
mgr.AddProcess(process);
//Wait for whatever
Thread.Sleep(10000);
//Kill all processes that were spawned by and including myExe.exe
mgr.TerminateAllProcessesInJob(exitCode:-100);
```

### 4. Launch a process and have it die when the parent process dies

```csharp
JobObject mgr = new JobObject();
mgr.SetJobLimits(new JobObjectLimit()
    .KillAllProcessOnJobObjectClose(true);
//Create a process suspended
//leaving it as an exercise to the reader.
//...
var process =   Process.Start("cmd.exe");
mgr.AddProcess(process);
//Wait for whatever
Thread.Sleep(10000);
//Kill me
System.Exit(0);
//You will notice note that cmd.exe is dead too
```
