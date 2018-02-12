# Fact Game

This is the source for [FactGame](fact-game.azurewebsites.net).

## Database Set Up

You will need a MongoDB instance somewhere. [mLab](https://mlab.com/) provides free ones. The database should be called `factgame`, and you need only one collection called `games`.

## Running

1. Clone/Download the source.
2. Open in Visual Studio.
3. Configure User Secrets.
	* In VS, right click `FactGame.Web` project and select "Manage User Secrets"
	* Add something similar to the following:
```
{
    "ConnectionStrings": {
        "FactGameData": "mongodb://<user>:<pass>@<server>:<port>/factgame"
    }
}
```
4. Run it.
