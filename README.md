
# GAMECURE

## Contributions
We welcome contributions to this project. Just fork the repo, make your changes
and submit a pull request and we'll get back to you. For more detailed instructions
head on over to GitHubs [Fork a repo](https://docs.github.com/en/get-started/quickstart/fork-a-repo])
docs.

## License
The GAMECURE project is dual-licensed.

* [Apache License, Version 2.0](https://www.apache.org/licenses/LICENSE-2.0).
* [MIT license](https://opensource.org/licenses/MIT).

As a user/licensee you choose which license to adhere to.

### Contribution
Unless you explicitly state otherwise, any contribution intentionally submitted
for inclusion in the work by you, as defined in the Apache-2.0 license, shall
be dual licensed as above, without any additional terms or conditions.

## Development - (work in progress)
### WIX

* Toolset: [Wix 3.14.0.5722](https://wixtoolset.org/releases/v3-14-0-5722/)
* Extension for VS 2022: [WixToolsetVisualStudio2022Extension](https://marketplace.visualstudio.com/items?itemName=WixToolset.WixToolsetVisualStudio2022Extension)


### Config file
To run the app locally you need to create a config.json file and place it in the root folder (you can also put it in the `[AppData]/GOALS/Gamecure` folder)
(This file is ignored in the .gitignore)


### Generate using build tool
The config file can be generated using the Gamecure build tool. For more info run this command.   
`dotnet run --project src/Gamecure.BuildTool -- --help`

| Exit code | Reason |
|---|---|
| -2 | There was an error parsing the arguments |
| -1 | There was an error executing the command |
| 0 | Success |


#### Manual
Template can be found in the root folder

[Template link](config_template.json)
*config_template.json*
```
{
    "project": "[Google project name]",
    "client_id": "[Client ID from Google Auth Client]",
    "container": "[Container where the editor is stored]",
    "token_url": "https://oauth2.googleapis.com/token",
    "auth_url": "https://accounts.google.com/o/oauth2/v2/auth",
    "scope": "https://www.googleapis.com/auth/devstorage.read_only",
    "plastic": {
        "includes": [
            "/Engine/Config",
            "/Engine/Shaders",
            "/Engine/Plugins",
            "/Engine/Build/BatchFiles",
            "/Engine/Build/Commit.gitdeps.xml",
            "/Engine/Binaries/ThirdParty",
            "/Engine/Binaries/DotNET/GitDependencies.exe",
            "/Engine/Binaries/DotNET/GitDependencies.exe.config",
            "/Game",
            "/Setup.bat"
        ],
        "excludes": [
            "/Game/Source",
            "/Game/Scripts"
        ]
    },
    "longtail": {
        "version": "v0.2.5"
    }
}
```
#### CI/CD implementation
The out of the box CI/CD Jenkins implementation will not work, you will have to comment out `argoWorkflowHelper.triggerWorkflow(....)` sections in helpers.groovy file and the `@Library(...)` in the JenkinsFiles to get it to work.
