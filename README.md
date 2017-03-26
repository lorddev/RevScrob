# RevScrob for Windows

This is a Windows service written in C# which uses the Last.fm REST API and the local iTunes COM library to keep 
your iTunes play counts and last played dates up-to-date if you listen to your music using another application which
supports scrobbling. It can be described as a "reverse scrobbler": instead of updating Last.fm counts with iTunes 
activity, it goes the other direction.

### Preserving local changes without affecting others

To update `app.config` without uploading your changes, go to the command-line
and enter:

    git update-index --assume-unchanged src/RevScrob/app.config

### Running the app

Currently there is no separate executable. To run the application, use the `BatchProcessor` unit tests.
I recommend to run the library-wide sync for the first run, and then to use the recent tracks sync
for subsequent passes.

### Known issues

* There is strange behavior when using an iPod before and after sync, with the Last.fm Scrobbler for Windows.
  If you sync the iPod and eject it, then run RevScrob to update your play counts, and listen to a few songs on
  the iPod, and then sync the iPod again, the Last.fm Scrobbler dialog box will show the songs that were updated
  in the previous syncing operation. You will need to make sure "Automatically Scrobble tracks from device" is unchecked,
  and uncheck the duplicates before submitting. Otherwise, syncing the duplicates will result in a feedback loop where
  the song is scrobbled every time you sync. If you accidentally scrobble the song, you can log in to Last.fm, find the
  song to fix, and delete the duplicate records.
