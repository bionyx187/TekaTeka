# Creating Mods

Please, keep in mind that every specification here is not definitive and is subject to change.\
Each mod or Song Pack it's saved on the folder `TekaSongs`, each one in a separate folder.

## Folder structure

All the files and folders inside the mod will have the same structure as the `StreamingAssets` folder of the original games, that is:

```
TekaSongs/
└── my-awesome-mod/
    ├── fumen/
    │   └── chart files
    ├── fumencsv/
    │   └── practice files
    ├── ReadAssets/
    │   └── song database
    └── sound/
        └── sound files
```

## Music files

Every file can be either Encrypted or Decrypted, and that TekaTeka will assume the later by using the file extension

| Folder/File type                  | Description                                                                                                                                            | Example                                                        | Encrypted Extension | Decrypted Extension |
| --------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------- | ------------------- | ------------------- |
| `fumen/{Chart file}`              | File containing the chart of the song, one for each difficulty                                                                                         | `fumen/clscam_e.bin` `fumen/clscam_h.bin` `fumen/clscam_m.bin` | `*.bin`             | `*.fumen`           |
| `fumencsv/{Practice Division}`    | CSV File containing the divisions used for practice mode                                                                                               | `fumencsv/clscam.bin`                                          | `*.bin`             | `*.csv`             |
| `sound/SONG_{Full song file}`     | This file contains the song file on the Criware proprietary format [ACB](https://game.criware.jp/manual/native/adx2_en/latest/criatom_basics_acb.html) | `sound/SONG_clscam.bin`                                        | `*.bin`             | `*.acb`             |
| `sound/PSONG_{Preview song file}` | File on the same proprietary format, but used for preview on the select song scree                                                                     | `sound/PSONG_clscam.bin`                                       | `*.bin`             | `*.acb`             |
| `ReadAssets/musicinfo.bin`        | Json file containing all the information about the songs like titles, file names, difficulties, etc.                                                   | `ReadAssets/musicinfo.bin`                                     | `musicinfo.bin`     | `musicinfo.json`    |

TekaTeka will have this priority when searching songs

```
1- Original file from the StreamingAssets folder
2- Decrypted Modded file
3- Encrypted Modded file
```
