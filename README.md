# Build
Run `nuke build` in the root directory. By default, this will be a release build with a signed .aab package for the google playstore distribution.
Use `--package-format apk` to build a release .apk version.

Feedback Form: 
- https://forms.gle/SDAERdx1JGny77EZ7

Websites: 
- https://devsmn.com
- https://devsmn.de

# Architecture
- Do not use dependency injection via reflection (e.g. IServiceXY as an argument in a constructor) as it's quite bad for performance on android. Instead, manually resolve services via `Services.GetService<TService>`
