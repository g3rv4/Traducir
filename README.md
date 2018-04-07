# traducir.win :unicorn:
A webapp to handle Transifex translations collaboratively using Stack Exchange accounts to log in.

## What is it?
We, on the Stack Overflow en español community have been struggling with keeping es.stackoverflow.com consistently in Spanish. This app should make translators and reviewers happy.

Users can go to traducir.win and do searches of existing strings and their current status. If they choose to log in (by using their Stack Exchange accounts) they can suggest translations. If they happen to be a mod, then they're automatically made reviewers.

## What technologies is it using?
The backend runs a .NET Core web application and the frontend is a React SPA. As a database, it uses SQL Server. There's a [docker image](https://hub.docker.com/r/g3rv4/traducir/) that's what's used to run this (and that image gets updated automatically on every push to `master`).

## I'd like to help, how can I set up a dev environment on my machine?
I'm glad you asked... [I've written a doc about it](https://github.com/g3rv4/Traducir/blob/master/docs/DEV_ENVIRONMENT.md)! If that doesn't work, feel free to [open an issue](https://github.com/g3rv4/Traducir/issues).

## Would you like to contribute?

A MA ZING! ZING! ZING! All kinds of contributions are accepted. Please read our [code of conduct first](https://github.com/g3rv4/Traducir/blob/master/docs/CODE_OF_CONDUCT.md) though. And before jumping to work on something, please discuss it on an issue.

## Who built it?
The first version was implemented by Gervasio Marchand. Gervasio (ugh, it's hard to write about myself in the third person) is currently a developer at Stack Overflow, but this project was started on his spare time and outside his dev role at Stack Overflow.

The idea is that the community takes over! So feel free to [create issues](https://github.com/g3rv4/Traducir/issues) or work on them. Before working on a PR, create an issue and make sure that it's a place where we're looking for contributions.