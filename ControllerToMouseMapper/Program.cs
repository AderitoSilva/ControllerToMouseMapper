using ControllerToMouseMapper.Input;
using XInputium.ModifierFunctions;
using XInputium.XInput;

// For this sample project, we are using our own simple implementation of mouse and
// keyboard interop. But for more complete and capable keyboard and mouse simulation
// features, check out: https://github.com/MediatedCommunications/WindowsInput/

// NOTE: To control the mouse and/or the keyboard when privileged applications have
// the focus, our application must also be run with privileges. Run this application
// as admin to ensure it can work on any application.

// NOTE: For the input loop, we are using a simple implementation of a precise timer,
// which is CPU intensive. In production, you should use something more appropriate,
// like a native media timer.

// Show some initial text in the console.
Console.WriteLine("This application maps the controller to the mouse. " +
    "This is just a demo application, with minimal functionality.");
Console.WriteLine();
Console.WriteLine("--Left joystick: mouse movement");
Console.WriteLine("--A: Left mouse button");
Console.WriteLine("--X: Right mouse button");
Console.WriteLine("--LS: Middle mouse button");
Console.WriteLine("--B: Escape key");
Console.WriteLine("--LB: Shift+Tab key");
Console.WriteLine("--LR: Tab key");
Console.WriteLine("--DPad: Arrow keys");

// =========================================================


// # SET UP THE INSTANCES THAT WILL DO ALL THE INPUT RELATED WORK

XGamepad gamepad = new();  // Our logical device. Our app is bound to this (i.e. subscribe to its events).
XInputDeviceManager deviceManager = new();  // We will use this to monitor all physical XInput devices.
deviceManager.DeviceStateChanged += (_, e) =>
{
    // The state of the [physical] device has changed. That includes the connection state, and user input.
    // If the device is connected, let's start using this physical device as our logical device.
    if (e.Device.IsConnected)
        gamepad.Device = e.Device;
};


// # CONFIGURE GAMEPAD AND SUBSCRIBE TO INPUT EVENTS

var mouseJoystick = gamepad.LeftJoystick;  // Joystick that controls the mouse.
var leftMouseButton = gamepad.Buttons.A;  // Gamepad button that emulates the left mouse button.
var rightMouseButton = gamepad.Buttons.X;  // Gamepad button that emulates the right mouse button.
var middleMouseButton = gamepad.Buttons.LS;  // Gamepad button that emulates the middle mouse button.

// Configure the joystick that controls the mouse.
mouseJoystick.InnerDeadZone = 0.2f;  // Let's use a circular dead zone.
mouseJoystick.RadiusModifierFunction = NonLinearFunctions.QuadraticEaseIn;  // Make the joystick more precise near the center.

// Map the joystick to the mouse.
double mouseMoveSpeed = 0.01;  // Mouse move speed, in screen ratio per second (kind of).
mouseJoystick.Updated += (_, _) =>
{
    // Update the mouse on every input loop tick. If the joystick is at rest position,
    // we don't need to move the mouse. Notice we are not using the joystick move event,
    // because we want the mouse to move while the joystick is being held (pushed),
    // even if the joystick is not moving.
    if (mouseJoystick.IsPushed)
    {
        // `MoveBy` method expects relative X and Y normal coordinates.
        // Also, we multiply by frame time to ensure the mouse speed is not affected
        // by the amount of time elapsed between each input loop iteration.
        Mouse.MoveBy(
            mouseJoystick.X * mouseMoveSpeed * mouseJoystick.FrameTime.TotalSeconds,
            -mouseJoystick.Y * mouseMoveSpeed * mouseJoystick.FrameTime.TotalSeconds);
    }
};

// Map controller buttons to the mouse buttons.
leftMouseButton.Pressed += (_, _) => Mouse.PressButton(MouseButton.Left);
leftMouseButton.Released += (_, _) => Mouse.ReleaseButton(MouseButton.Left);
rightMouseButton.Pressed += (_, _) => Mouse.PressButton(MouseButton.Right);
rightMouseButton.Released += (_, _) => Mouse.ReleaseButton(MouseButton.Right);
middleMouseButton.Pressed += (_, _) => Mouse.PressButton(MouseButton.Middle);
middleMouseButton.Released += (_, _) => Mouse.ReleaseButton(MouseButton.Middle);

// Map controller buttons to keyboard keys.
// For these buttons (which map to keyboard keys), we would like them to repeat
// if the user holds the button. For repeating the gamepad button, we will use
// dynamic events (in this case, `RepeatDigitalButtonInputEvent`).
TimeSpan buttonRepeatDelay = TimeSpan.FromMilliseconds(350);  // When a button is held, how long until it starts repeating?
TimeSpan buttonRepeatInterval = TimeSpan.FromMilliseconds(90);  // While a held button is repeating, how long is the interval?
gamepad.RegisterButtonRepeatEvent(XButtons.DPadUp, buttonRepeatDelay, buttonRepeatInterval,
    (_, _) => Keyboard.TapKey(KeyboardVirtualKey.Up));
gamepad.RegisterButtonRepeatEvent(XButtons.DPadDown, buttonRepeatDelay, buttonRepeatInterval,
    (_, _) => Keyboard.TapKey(KeyboardVirtualKey.Down));
gamepad.RegisterButtonRepeatEvent(XButtons.DPadLeft, buttonRepeatDelay, buttonRepeatInterval,
    (_, _) => Keyboard.TapKey(KeyboardVirtualKey.Left));
gamepad.RegisterButtonRepeatEvent(XButtons.DPadRight, buttonRepeatDelay, buttonRepeatInterval,
    (_, _) => Keyboard.TapKey(KeyboardVirtualKey.Right));

gamepad.RegisterButtonRepeatEvent(XButtons.LB, buttonRepeatDelay, buttonRepeatInterval,
    (_, _) =>
    {
        // Shift+Tab
        Keyboard.PressKey(KeyboardVirtualKey.Shift);
        Keyboard.TapKey(KeyboardVirtualKey.Tab);
        Keyboard.ReleaseKey(KeyboardVirtualKey.Shift);
    });
gamepad.RegisterButtonRepeatEvent(XButtons.RB, buttonRepeatDelay, buttonRepeatInterval,
    (_, _) => Keyboard.TapKey(KeyboardVirtualKey.Tab));

gamepad.Buttons.B.Pressed += (_, _) => Keyboard.TapKey(KeyboardVirtualKey.Escape);


// # START LISTENING FOR INPUT

// Start the input loop. Note that, from now on, any changes you may want to make to
// the gamepad must always be done within the input loop update callback, to ensure
// thread safe access to the gamepad. The input loop guarantees that our callback
// is always called safely. Thread safety here is crucial, because `XGamepad` is
// not expected to be thread safe.
using InputLoop inputLoop = new(time =>
{
    // By updating the device manager, all its devices are also updated.
    // So, we don't need to update our `XGamepad`, because it is using
    // a device from the device manager.
    deviceManager.Update();  // All input events will fire here.
});
inputLoop.Start();  // Start our input loop, to listen for input events.


// =====================================

// Wait for the user to press any key on the console, so the console remains open.
Console.WriteLine();
Console.WriteLine("Press any key to terminate the application.");
Console.ReadKey();


// -------------------------------------
// HOW THIS APPLICATION WORKS
// -------------------------------------

/*
 * Let's understand how this little app works. Because this sample is focused on
 * understanding how to use XInputium, let's forget about how the code that simulates
 * the mouse and keyboard works for a moment.
 * 
 * The goal is to have our XInput controller simulating the mouse and the keyboard.
 * We also want the user to be able to use any gamepad they may have connected in
 * their system. Doesn't matter how many gamepads they have connected -- if they
 * interact with any gamepad in any way, we want that interaction to have its
 * respective effect.
 * 
 * The `XGamepad` class represents a logical XInput device, while `XInputDevice`
 * represents an actual physical XInput device. An `XGamepad` instance wraps an
 * `XInputDevice` instance, and we can change its wrapped instance at any time.
 * This allows us to use one single `XGamepad` instance in our application to
 * represent a device, but change the physical device bound to it at any moment.
 * So, this way, we can safely depend on a single `XGamepad` instance across our
 * application (for example, subscribe to its events and refer to it in our code),
 * without worrying that we might need to update these dependencies if the gamepad
 * gets disconnected of if the user starts using another gamepad. We code our
 * application for an `XGamepad` instance, and just change its `Device` property
 * to use whatever `XInputDevice` we want whenever we want, so our code is
 * decoupled from a specific physical device.
 * 
 * This application uses an `XGamepad` instance. It is that instance that does all
 * the heavy work of detecting input state changes, like button presses, joystick
 * movements and more. We use that instance for most of our code -- we subscribe to
 * its events or the events of its members to get notified of specific interactions
 * the user performs with the gamepad.
 * 
 * `XGamepad` doesn't know what physical input device you want to use, so it won't
 * take any initiative of switching between different devices by itself. It leaves
 * that decision to you. We want its device to always be the one the user interacted
 * with more recently, so we need to update the `XInputDevice` instance used by
 * our `XGamepad` accordingly.
 * 
 * First, to know what devices are connected and what the user is interacting with,
 * we need to monitor all devices in the system for changes. Those changes include
 * input from the user and connected/disconnected changes. That's a job for the
 * `XInputDeviceManager`. With an instance of the `XInputDeviceManager` class, we
 * can monitor the state of all physical XInput devices at once. It offers events
 * that notify us when the state of any device changes, and when it gets connected
 * or disconnected. Our application uses `XInputManager` for that.
 * 
 * `XInputManager` provides us with singleton `XInputDevice` instances, and we will
 * use only those for our `XGamepad`. By subscribing to our `XInputManager`'s
 * `DeviceStateChanged` event, we get notified whenever a device is connected
 * or disconnected, and whenever there is any input from the user in a device. This
 * event's arguments also provide us the `XInputDevice` that represents the device
 * that just changed. So, in our application, we subscribe to that event, and,
 * when it fires, we start using affected device in our `XGamepad` instance, unless
 * the event fired because the device just got disconnected. This way, we ensure
 * that the device used by our `XGamepad` is always the connected device that
 * the user interacted with lastly.
 * 
 * Our application subscribes to several events of our `XGamepad` instance and
 * its members. This enables us to get notified of user input actions, like
 * button presses and other input. Most code in this file is actually just for
 * subscribing events. However, you may be wondering how will our `XGamepad`
 * instance know when and how often to communicate with the physical device to
 * get the device's state. `XGamepad` relies on its `XInputDevice` to tell it
 * when something changed, and `XInputDevice` relies on your code to tell it
 * when it must check the physical device for changes. `XInputDevice` exposes
 * the `Update()` method that gets fresh information from the device, and is
 * also that method that triggers any input related events in the API. For
 * convenience, `XGamepad` also exposes an `Update()` method that, in turn,
 * calls `XInputDevice.Update()` on its wrapped device. If you call
 * `XInputDevice.Update()` directly, you don't need to call `XGamepad.Update()`
 * method. They both do the same. Likewise, `XInputDeviceManager` also exposes
 * an `Update()` method, which calls `XInputDevice.Update()` on all its devices.
 * And, because we are using the manager to monitor all devices in the system,
 * we can simply call its `Update()` method, and we are sure
 * that it will update its devices, including the one our `XGamepad` is using.
 * So, in short, in our application, we opt for calling the `Update()` method
 * of our `XInputDeviceManager`, and any state change will be automatically
 * reflected in our `XGamepad`.
 *
 * Now, you know how the devices are updated and what triggers the updates.
 * However, you might be asking yourself when are they updated. Calling any of
 * the `Update()` methods mentioned above, is always done from your code. Neither
 * `XGamepad`, nor `XInputDevice`, nor `XInputDeviceManager` will ever do it
 * automatically. The decision of when the updates should occur is yours,
 * because you know best. If our application had a UI, the updates could be
 * requested, for example, at every frame of the UI render engine, several times
 * per second. But because ours is a console app, which has no concept of UI or
 * render loop, we need something else that can provide us with a loop that
 * iterates several times per second, just like a render engine would. For our
 * application, we are using an `InputLoop` instance, which provides exactly
 * that. `InputLoop` ticks several times per second at constant rate (the default
 * is 60). However, `InputLoop` still doesn't trigger updates on the devices on
 * its own. Instead. it calls a callback provided by us, which is responsible
 * for triggering the updates, and that's exactly how our application does it.
 * In this callback, our application calls the `XInputDeviceManager.Update()`
 * method, which in turn causes the state of every device to be updated with
 * fresh information, immediately firing every input event as needed.
 *
 * So, let's wrap up how the whole input system is set up:
 * 
 *   * We use an `XGamepad` instance that represents a logical input device,
 *     and is responsible for providing us with input events from a physical
 *     input device.
 *   * We use an `XInputDeviceManager` to monitor all physical devices in the
 *     system for changes, so the user can use any controller, not just one.
 *   * Our `XInputDeviceManager` provides our `XGamepad` with the physical
 *     device it should use, and our code decides what device and when.
 *   * We use an `InputLoop` instance that calls our callback 60 times per
 *     second. Our callback requests our `XInputDeviceManager` to update all
 *     its devices, which in turn causes any input events to fire.
 *   * We subscribe to some events of `XGamepad` and its members, which notify
 *     us about specific user input actions. When these events are fired, we
 *     simulate the respective mouse or keyboard actions.
 *
 * Now that you better understand the control flow of the application, we should
 * talk a little bit about the input events.
 * 
 * In the code above, you may notice that we are subscribing to some events,
 * like the `Pressed` or `Released` events of the buttons, and a few others.
 * Those are regular .NET events (CLR events), like you would find in most event
 * based .NET applications or libraries. You may also notice that we are also
 * using another way for registering some of the events, like the ones subscribed
 * with a call to `RegisterButtonRepeatEvent()` method. These ones are not
 * regular events; they are dynamic events, provided by XInputium. Unlike
 * regular .NET events, which fire when a well known action occurs, dynamic
 * events are created on demand and fire when a specific condition provided
 * or configured by you occurs. In the case of our application, we are using a
 * particular kind of dynamic event, which fires repeatedly at specified
 * intervals while the user holds a specified button for a specified amount of
 * time. These conditions are very specific and are configurable, so it would
 * not be possible to have a regular .NET event with this logic. Dynamic events
 * solve the conditional limitation of regular .NET events.
 * 
 * XInputium provides several dynamic events, and you can even create your
 * own. Our little app uses just the ones it needs, but feel free to use other
 * ones if you'd like.
 */

