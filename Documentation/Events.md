# Events.cs
This class contains all the properties of an event.
It inherits from Sprite.

-----

## Class Fields
| Name          | Type   | Explanation   |
| :-----------  | :----- | :------------ |
| busID         | `int`    | Contains the ID of the bus an event is going to affect.         |
| eventID   | `int` | Contains the ID of the chosen event.             |
| start   | `float` | Contains the game time at which the event is going to start.             |
| end   | `float` | Contains the game time at which the event is going to end.              |
| p   | `double` | Contains the amount of change accomplished by an event.              |
| text   | `string` | Contains the description to an event.

------
## Class constructor

```cs
public Events(int eventID, int busID, double p, float start, float end, string text)
        {
            this.ID = eventID;
            this.BusID = busID;
            this.P = p;
            this.T1 = start;
            this.T2 = end;
            this.Text = text;
        }
```

#### Input Arguments
 - `eventID`: The ID of the event according to which an event type is chosen. The list of the IDs can be found in the `level template`.
 - `busID`: The number of the bus the event is going to affect if the eventID is 1. The ID of the line which is affected if the eventID is 6 or 7.
 - `p`: The amount of power which the buses change at the event. By wind and photovoltaic events, it is the factor of the maximal power.
 - `start`: The game time at which the event occurs.
 - `end`: The game time at which the event ends. It is relevant for the wind and photovoltaic events.
 - `text`: The text which is shown when the event happens.

#### Description
The constructor sets the values of the properties to the current appropriated input arguments.


------

## Properties

### ID
```cs
public int ID
```

#### Set
Sets its value to eventID.

#### Get
Returns the value of eventID.

------
### P
```cs
public double P
```

#### Set
Sets its value to p.

#### Get
Returns the value of p.

------
### T1
```cs
public float T1
```

#### Set
Sets its value to start.

#### Get
Returns the value of start.

------
### T2
```cs
public float T2
```

#### Set
Sets its value to end.

#### Get
Returns the value of end.

------
### BusID
```cs
public int BusID
```

#### Set
Sets its value to busID.

#### Get
Returns the value of busID.

------
### Text
```cs
public string Text
```

#### Set
Sets its value to text.

#### Get
Returns the value of text.





