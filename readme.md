InstaFeed
=========

Use this tool to generate single feed from more than one Instapaper folder. This is command line utility, but set of classes could be easily extended and reused if needed.

Requirements to compile
-----------------------

Solution was created in Visual Studion 2012 and could be compiled in [Visual Studio Express 2012 for Windows Desktop](http://www.microsoft.com/visualstudio/eng/products/visual-studio-express-for-windows-desktop) version.

Command line parameters
-----------------------

Utility can be parametrized. Full list of available parameters:

    -c, --command        Required. Specifies command or comma separated list of
						 commands: list|rss|archive. Required.
    -u, --username       Specifies Instapaper account username. Required.
    -p, --password       Specifies Instapaper account password. Required.
    -d, --directories    Comma separated list of directories to include.
                         Names case insensitive.
                         If not specified all directories are processed.
    -o, --output         Specifies output file name and destination for RSS feed. Equal to .\feed.rss by default.
                         Can contain environment variables.
    -h, --help           Displays help screen.

Usage Examples
--------------

Get list of Instapaper directories:

    instafeed -c list -u johndoe@domain.com -p johndoepasswd
    instafeed --command list --user johndoe@domain.com --password johndoepasswd

Generate rss feed from _Read Later_ and _My Custom Dir_ directories and save it to _%TEMP%_ folder:

    instafeed -c rss -u johndoe@domain.com -p johndoepasswd -d "Read Later, My Custom Dir" -o %TEMP%\feed.rss
    instafeed --command rss --username johndoe@domain.com --password johndoepasswd --directories "Read Later, My Custom Dir" --output %TEMP%\feed.rss

Archive all items in _Read Later_ and _My Custom Dir_ directories:

    instafeed -c archive -u johndoe@domain.com -p johndoepasswd -d "Read Later, My Custom Dir"
    instafeed --command archive --username johndoe@domain.com --password johndoepasswd --directories "Read Later, My Custom Dir"

Generate feed from all directories, add datestamp to the output file and archive all items in all directories.

    instafeed -c "rss, archive" -u johndoe@domain.com -p
johndoepasswd -o "[%date:/=%]_instapaper_unread_articles.rss"

