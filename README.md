# vibranceGUI

VibranceGUI is a Windows Utility written in C# that automates NVIDIAs Digitial Vibrance Control and AMDs Saturation for Games, e.g. Counter-Strike: Global Offensive by utilizing native graphic card driver APIs.

As of 18th April 2015, vibranceGUI also fully supports AMD graphic cards. Prior to that, vibanceGUI was developed to support NVIDIA graphic cards only.

Note that NVIDIA Laptop GPUs are not supported because their drivers do not contain the needed functionality.
Intel did not publish an API for their integrated GPUs and are not supported.

More information and binary download available at: http://vibrancegui.com/

## Troubleshooting (v2.5.0+)

If you see the "Both NVIDIA and AMD graphic drivers have been found on your system" error (for example on systems with an AMD iGPU and a dedicated NVIDIA GPU), you can force the GPU type via a shortcut: create a shortcut to `vibranceGUI.exe`, open its Properties, and append a space plus `--force-nvidia` or `--force-amd` to the Target path (e.g. `C:\WHATEVER\vibranceGUI\vibranceGUI.exe --force-nvidia`).

## Compiling

When compiling, make sure to compile for x86 target platform.

## Contributing

Every contribution is greatly appreciated. Do not hesitate to submit every issue and pull request that comes to your mind.

## Contact

NVIDIA support: https://twitter.com/juvlarN

AMD support: https://twitter.com/juRiiir3

`Please do not add either of us at Steam to ask questions about vibranceGUI. Thank you.`
