## PrimeApps Runtime
You can easily install the PrimeApps Runtime to your local machine using this repository.

### Prerequisites
* [ASP.NET Core Runtime 2.2](https://dotnet.microsoft.com/download/dotnet-core/2.2)
* [Windows only] Bash for Windows (Git Bash, Cygwin, Msys2, etc). We recommend [Git Bash](https://github.com/git-for-windows/git/releases)
* [Windows only] [Microsoft Visual C++ Redistributable](https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads)

### Setup
1. [Clone this repository](#1-clone-this-repository)
2. [Run install.sh](#2-run-installsh)
3. [Run run.sh](#3-run-runsh)

#### 1. Clone this repository
```bash
git clone https://github.com/primeapps-io/runtime.git
```

#### 2. Run install.sh
```bash
cd runtime/setup
./install.sh
```

#### 3. Run run.sh
```bash
cd ../startup
./run.sh
```
