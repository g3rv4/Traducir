# traducir.win :unicorn:
A webapp to handle Transifex translations collaboratively using Stack Exchange accounts to log in.

## What is it?
We, on the Stack Overflow en español community have been struggling with keeping es.stackoverflow.com consistently in Spanish. This app should make translators and reviewers happy.

Users can go to traducir.win and do searches of existing strings and their current status. If they choose to log in (by using their Stack Exchange accounts) they can suggest translations. If they happen to be a mod, then they're automatically made reviewers.

## How to use traducir.win?

If you didn't yet please read [Traduciendo el sitio… Esta vez, con más control sobre el proceso
](https://es.meta.stackoverflow.com/q/3378).

Go to https://traducir.win. You will found that the user interface is pretty intuitive. Just bear in mind that not all the strings should be translated, only those that are found on the user interface of target Stack Exchange site should be. At this time https://traducir.win only has [Stack Overflow en español](https://es.stackoverflow.com) as a target site.

To learn the ropes of app follow the [tour](/docs/TOUR.md)

## What technologies is it using?
The backend runs a .NET Core web application and the frontend is a React SPA. As a database, it uses SQL Server. There's a [docker image](https://hub.docker.com/r/g3rv4/traducir/) that's what's used to run this (and that image gets updated automatically on every push to `master`).

## I'd like to help, how can I set up a dev environment on my machine?
I'm glad you asked... [I've written a doc about it](https://github.com/g3rv4/Traducir/blob/master/docs/DEV_ENVIRONMENT.md)! If that doesn't work, feel free to [open an issue](https://github.com/g3rv4/Traducir/issues).

## Would you like to contribute?

A MA ZING! ZING! ZING! All kinds of contributions are accepted. Please read our [code of conduct first](https://github.com/g3rv4/Traducir/blob/master/docs/CODE_OF_CONDUCT.md) though. And before jumping to work on something, please discuss it on an issue.
