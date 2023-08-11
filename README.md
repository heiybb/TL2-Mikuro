# TL2-Mikuro

## Why Build This
Each time I attempt to pack/unpack a MOD, have to launch the Editor and endure a 20-second wait as it loads numerous unrelated functions.  
TL2-Mikuro, which takes merely 3 seconds to initialize.  
But It accomplishes this by directly invoking functions from `EditorGuts.dll` hence regrettably, `EditorGuts` is still a prerequisite for this.

## Installitaion
Just drop the `TL2-Mikuro.exe` into the same folder of `Editor.exe/Torchlight2.exe`.  

## Known Issue
Limited by the logic inside `EditorGuts.dll`, MOD pack/build only accept the project in `mods` folder, and MOD unpack only accpet target directory into `mods` as well.  
I'm new to WPF and C# programming, so please excuse any shortcomings in my code.

## TO-DO
Further analysis in `EditorGuts.dll` to see if we can make it better.

## License
This project is licensed under the [GNU General Public License v3.0](https://www.gnu.org/licenses/gpl-3.0.en.html).