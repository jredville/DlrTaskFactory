Msbuild support for Dlr language inline tasks.

Sample MSBuild file:
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="4.0"
         DefaultTargets="Build"
         xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <UsingTask
    TaskName="HelloWorld"
    TaskFactory="DlrTaskFactory"
    AssemblyFile="$(TaskFactoryPath)DlrTaskFactory.dll">
    <ParameterGroup>
      <Name Required="true"/>
      <TaskMessage Output="true"/>
    </ParameterGroup>
    <Task>
      <Code Type="Fragment"
            Language="rb">
        <![CDATA[
        self.task_message = "Hello #{name} from Ruby".to_clr_string
        log.log_message(task_message);
        ]]>
      </Code>
    </Task>
  </UsingTask>
  <PropertyGroup>
    <YourName Condition=" '$(YourName)'=='' ">Jim</YourName>
  </PropertyGroup>

  <Target Name="Build">
    <HelloWorld Name="$(YourName)">
      <Output PropertyName="RubyOut"
              TaskParameter="TaskMessage"/>
    </HelloWorld>
    <Message Text="Message from task: $(RubyOut)"
             Importance="high" />
  </Target>
</Project>

And it's output:
[V10|(master*) ~\D\S\D\D\b\Debug] 111> msbuild .\test.proj
Microsoft (R) Build Engine Version 4.0.30319.1
[Microsoft .NET Framework, Version 4.0.30319.1]
Copyright (C) Microsoft Corporation 2007. All rights reserved.

Build started 4/27/2010 10:33:30 AM.
Project "C:\Users\jdeville\Desktop\Src\DlrTaskFactory\DlrTaskFactory\bin\Debug\
test.proj" on node 1 (default targets).
Build:
  Hello Jim from Ruby
  Message from task: Hello Jim from Ruby
Done Building Project "C:\Users\jdeville\Desktop\Src\DlrTaskFactory\DlrTaskFact
ory\bin\Debug\test.proj" (default targets).


Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:01.21

TODO: Proper support for Rake, easier addition of new languages, file and class based inline task support.