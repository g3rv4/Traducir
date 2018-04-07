# Setting up a local environment

Do you want to help writing beautiful code? THANK YOU THANK YOU THANK YOU <3

## Requirements

In order to run this project on your computer entirely, you're going to need:

1. An IDE compatible with .NET, Typescript and JSX. I love [Visual Studio Code](https://code.visualstudio.com/) and that's what I've used to develop the first version... entirely on MacOs.
   * I use the following extensions that make my life so much better:
       * [ASP.NET Helper](https://marketplace.visualstudio.com/items?itemName=schneiderpat.aspnet-helper)
       * [C#](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp)
       * [C# Extensions](https://marketplace.visualstudio.com/items?itemName=jchannon.csharpextensions)
       * [C# FixFormat](https://marketplace.visualstudio.com/items?itemName=Leopotam.csharpfixformat)
2. A SQL Server instance. You can use Microsoft's docker image. [I put together some instructions to set it up using docker-compose](https://github.com/g3rv4/Traducir/blob/master/docs/MSSQL_DOCKER.md).
3. A [StackApp](https://stackapps.com/) that has `localhost:8080` as the OAuth Domain.
4. [.NET Core](https://www.microsoft.com/net/) and [Node.js](https://nodejs.org/en/) set up.
5. A Transifex account with access to the project you want to work with. I'm assuming it's SOes because it's the first one. You're going to need an API key that you can generate [on their site](https://www.transifex.com/user/settings/api/).

For the C# FixFormat extension to make me happy, I'm using the following settings. Adhering to them would be greatly appreciated.

```
"csharpfixformat.style.braces.onSameLine": false,
"csharpfixformat.style.spaces.afterParenthesis": false,
"csharpfixformat.style.spaces.beforeParenthesis": false,
"csharp.suppressHiddenDiagnostics": false,
```

## Initial setup

### Create the database
Create a database named `Traducir` (it could be whatever, just know that I'm assuming that name) on your SQL Server instance.

It can be accomplished by running `Create Database Traducir` (yes, without uppercases because we don't yell at our database server). Also, I'm using `sa` on my machine... if you don't want to for whatever reason, create a user for the application and make sure it can do anything on that database.

### Set up the backend on Visual Studio Code

Open the project and go to Debug => Start Debugging. The first time it should ask you to add some files inside of a `.vscode` folder. Just tell it to go ahead and do it.

Stop it now... we need to configure the environment variables. Everything is set up using environment variables so that it's easy to keep track of them and to make it easy to set up when running the project inside a docker container (that's exactly how I'm running it on prod... and how I'll run it when we have tests. Yes, we will have tests some day).

Open the `.vscode/launch.json` and replace the existing `env` key with the following (changing whatever is between `<>` with the appropriate values for you):

```
"env": {
    "ASPNETCORE_ENVIRONMENT": "Development",
    "FRIENDLY_NAME": "SOes",
    "CONNECTION_STRING": "Server=<SQL SERVER ADDRESS>;Database=Traducir;User Id=<SQL SERVER USER>;Password=<SQL SERVER PASSWORD>;Min Pool Size=5;",

    "USE_HTTPS": "False",
    "STACKAPP_SECRET": "<YOUR STACKAPP SECRET>",
    "STACKAPP_CLIENT_ID": "<YOUR STACKAPP CLIENT ID>",
    "STACKAPP_KEY": "<YOUR STACKAPP KEY>",
    "STACKAPP_SITEDOMAIN": "es.stackoverflow.com",

    "TRANSIFEX_APIKEY": "<YOUR TRANSIFEX API KEY>",
    "TRANSIFEX_RESOURCE_PATH": "api/2/project/stack-overflow-es/resource/english/translation/es/strings/",
    "TRANSIFEX_LINK_PATH": "stack-exchange/stack-overflow-es/translate/#es/english"
},
```

Now... if you run it, it should work. Visit `http://localhost:5000/app/api/admin/migrate` to run the migrations. That should create all the tables the system uses for you (or run migrations that were created since you last pulled). If you visit `http://localhost:5000/app/api/admin/pull` that should populate the strings on your database. Give it a try :)

### Set up the frontend

On the terminal, go to whatever the project is set up and then into the `Traducir.Web` folder. Then type `npm install`. It should take a little while to install the javascript dependencies, but it's only once.

## Running it

Once you were able to run the project from VS Code, the next time you open the project you can just press F5 or got o Debug => Start Debugging to start the backend.

And to run the frontend, you can just go to the `Traducir.Web` folder on the console and write `npm start` (now that I'm writing it, I'm sure there's a way to integrate it with VS Code... I'll look into that unless somebody beats me to it).

And that's it! if you go to `http://localhost:8080` everything should work as expected. You should be able to do searches. And if you click on "Log In", that should work as well.

If you want to test being a trusted user or a reviewer, you can just change that on the database... but you should log out and log in again in order to see the changes.

**WARNING:** Pushes to transifex are going to fail for you unless you are a reviewer on their system. If you *are*, I strongly suggest that you don't use your main account's API key, as you will end up pushing trash to Transifex. *This only applies to site moderators*.