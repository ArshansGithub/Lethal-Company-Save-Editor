# Lethal-Company-Save-Editor

A Lethal Company save editor that is open source, built-in C#, .NET 8.0, using WPF. If you are looking for a download/release -> [click me](https://github.com/ArshansGithub/Lethal-Company-Save-Editor/releases/) 
https://youtu.be/GG6tZs23O1Q
![LCSaveEditor_QxRnPOCVzw](https://github.com/ArshansGithub/Lethal-Company-Save-Editor/assets/111618520/2495d997-6c6d-4e08-b669-5a6aeaf93881)

## Table of Contents
- [Background](#background)
- [Features](#features)
- [Goals](#goals)
- [Usage](#usage)
- [Contributing](#contributing)

## Background
This project was created because I was curious about how Lethal Company worked as a whole so I began taking apart piece by piece. Their save system intrigued me and looking at current Lethal Company save editors I saw a lack of user-friendly design and a lack of features within these save editors.

## Features

- Automatic or Manual save folder finder. Automatic looks for the save files in their default directory while manual allows you to pick a folder to search
- Adapative save file picker. If your folder has multiple or perhaps only a single save file, the program does not force you to have all 3 saves in the same folder allowing you to copy a specific save file to an isolated folder and tinker with it there safely.
- Feature-rich including player stat editor, ship unlock editor, game stats editor, and scrap/item editor.
- User-friendly UI
- Decide between saving the modified save file to a different directory or directly to the game's save files (through automatic finding)
- And other QOL features behind the scenes

### Goals
- [ ] Make a raw editor for mods that use the same save system the game does
- [ ] Code refactoring because I rarely use C# or .NET 

## Usage
Ensure you have .NET 8.0 installed, simply download the latest release, run the executable, and enjoy tinkering with your saves :)

## Contributing

Contributions are welcome. You can contribute by reporting issues, suggesting improvements, or submitting pull requests. To get started:

    Fork this repository.
    Create a new branch for your feature or bug fix: git checkout -b feature/your-feature.
    Make your changes and commit them.
    Push to your fork: git push origin feature/your-feature.
    Create a pull request.

Please ensure your code adheres to proper coding standards and includes relevant tests.
