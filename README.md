# Requirements
Implement a system that monitors currently running processes.
Add simple user interface (using any technologies like WPF, WinForms, Web app) that shows gathered information.

# Development
There are some ways for getting gathered information like process id, name, CPU usage and owner.
For process id and name we can monitor this by using the  System.Diagnostics namespace. 
For calculating the CPU usage of processes we need to get a value that indicates how much time they have used the processor in a certain period of time, this value is equal to the sum of the time that kernel and user have spent on these processes, first way use also  System.Diagnostics namespace. The Process class has a property called TotalProcessorTime.TotalMilliseconds which gives us how much time the processor has spent on this process, but it is work only for pre .NET 2.0 (access denied when reading  TotalProcessorTimem of the idle-process).
Other way is using the GetSystemTimes() function. This function returns parameters CreationTime, ExitTime, KernelTime and UserTime. KernelTime and UserTime are equivalent to the managed version.

# User Interface
WPF was chosen for adding simple user interface.
WPF DataGrid control in XAML provides a flexible way to display a collection of data in rows and columns. This control has many convenient functions by default, but need to fix scroll issue when virtualisation is enabled. In this case, it is better to choose Web app technology.
MVVM suspects present — Views,  ViewModel and ProcessManager. Also was added some helper classes.
