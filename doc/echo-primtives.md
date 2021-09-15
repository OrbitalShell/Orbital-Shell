# Echo Primitives

``Echo Primitives`` are code units that transforms a data to an ANSII/VT100 characters sequence.

## call paths

report of main cases:

| caller | subcaller | type handled | echo method |
| -- | -- | -- | -- |
**var** || IDataObject | ``List<IDataObject>.Echo``
||| DataValue | ``IDataObject.Echo``
||| object | ``EchoPrimitives.DumpObject``
**env**<br>**globals**<br>**locals** | ShellCommands_Var<br>.DumpVarTable | IDataObject | ``List<IDataObject>.Echo``
||| DataValue | ``IDataObject>Echo``
||| object | ``EchoPrimitives.DumpObject``
**echo** || string | ``context.Out.Echo``
||| object | ``object.Echo``
|||| ``{byType}.Echo``

report for sub methods:

| caller | subcaller | type handled | echo method |
| -- | -- | -- | -- |
**DumpObject** | DataTable.Echo<br>GetVarsDataTable<br>AddIDataObjectToTable<br>AddObjectToTable | null | ``DumpAsText``
||| DataValue | ``object.Echo``
|||| ``{byType}.Echo``

## output decoration layers

| level | name | location
|-- | -- | -- |
| 2 | echo primitives | **kernel**,OrbitalShell.Component.Console.``EchoPrimitives``<br>``ShellObject`` *(base object)*<br>``AsText()``<br>``ToString()``
| 1 | echo directives | **console-app**,OrbitalShell.Component.Console.``ConsoleTextWriterWrapper``<br>**console-app**,OrbitalShell.Component.EchoDirective.``EchoDirectiveProcessor``
| 0 | ANSI | **console-app**,OrbitalShell.Component.Console.``ANSI``<br>**console-app**,OrbitalShell.Component.Parser.ANSIParser.``ANSI``
| 0 | ASCII | OrbitalShell.Component.Console.``ASCII``
| 0 | Unicode | **console-app**,OrbitalShell.Component.Console.``Unicode``

<br>
<br>

<hr>

Orbital Shell, 14 september 2021