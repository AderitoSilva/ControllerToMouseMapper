# ControllerToMouseMapper

This repository provides a basic sample console application project that demonstrates how [XInputium](https://github.com/AderitoSilva/XInputium) can be used to emulate the mouse and keyboard.

All XInputium related code is in the `Program.cs` file, and it showcases the main focus of XInputium, which is to make input related code as simple and straightforward as possible, abstracting input state management from the consumer.

The sample project also shows how one can implement a thread-safe input loop based on a precise timer. This might be useful in scneraios where you don't have a UI system that manages a loop for you, like in the case of a console application or a service. Although this precise timer implementation does its job, it is based on a spinner, so it is CPU intensive. A better alternative would be to use a native precise timer, like those provided by Windows media timers.

`Program.cs` file has detailed comments that explain what each block does and why it does it. It also has a comment section that explains in more detail what is being done and why. You are encouraged to read that comment section to better undertsand, not just this particular application, but how to use XInputium in practice.
