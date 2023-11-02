# EvoSC# Tool
This is mainly a development tool for [EvoSC#](https://github.com/EvoEsports/EvoSC-sharp). It simplifies certain tasks such as creating and setting up a development environment for new modules, both internal and external. It can also create migrations with the correct conventions.

# Installation
Because the tool will only run within the EvoSC# project, it is recommended to install it as a local tool. Therefore, while inside the EvoSC# solution, install by:
```bash
dotnet tool install EvoSC.Tool
```

To install it as a global tool:
```
dotnet tool install --global EvoSC.Tool
```

# Usage
You can access the tool with the following command:
```bash
dotnet evosc
```

If it is install as a *global* tool, the command is simply:
```bash
evosc
```
