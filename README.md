# RevScrob for Windows

This is a Windows service written in C# which uses the Last.fm REST API and the local iTunes COM library to keep 
your iTunes play counts and last played dates up-to-date if you listen to your music using another application which
supports scrobbling. It can be described as a "reverse scrobbler"; instead of updating Last.fm counts with iTunes 
activity, it goes the other direction.

### Preserving local changes without affecting others

To update `app.config` without uploading your changes, go to the command-line
and enter:

    git update-index --assume-unchanged src/RevScrob/app.config
