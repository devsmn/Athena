# Build
## Prerequisites
There are three special files that are required for the build to work:
- app.secrets.debug.props
- app.secrets.dev.props
- app.secrets.prod.props

### app.secrets.debug.props
This file contains the ad unit ids that are used while developing. We use the official deveopment ad unit ids provided by google. 
> This file is provided by the repository and does not have to be adjusted.

### app.secrets.dev.props
This file contains the syncfusion license assigned to an individual developer. You need to get your own license at [Syncfusion](https://www.syncfusion.com/).
> This file needs to be added manually by each developer. It is required for development.

### app.secrets.prod.props
This file contains the production app unit id.
> This file is only used for release builds, so it should not be needed during development.

Run `nuke build` in the root directory. By default, this will be a release build with a signed .aab package for the google playstore distribution.
Use `--package-format apk` to build a release .apk version.

# Development
## General
Do not use dependency injection via reflection (e.g. IServiceXY as an argument in a constructor) as it's quite bad for performance on android. Instead, manually resolve services via `Services.GetService<TService>`.

## Java runtime bindings
## Build slim interop library via gradle
Navigate to the `native` subfolder of the java module (e.g `./documentscanner/native/)` and run `.\gradlew.bat assembleRelease` to build the binaries. Run `.\gradlew.bat build --refresh-dependencies` to refresh dependencies.

### Java source files
Ensure that java source files are located in the related native folder. Adding them to the runtime binding project will place the files in the wrong directory. Instead, create them manually and add them as a **link** to the project.

# Links

Feedback Form: 
- https://forms.gle/SDAERdx1JGny77EZ7

Websites: 
- https://devsmn.com
- https://devsmn.de

