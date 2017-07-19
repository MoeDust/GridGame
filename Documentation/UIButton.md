# UIButton.cs


-----

## Class Fields
| Name          | Type   | Explanation   |
| :-----------  | :----- | :------------ |
| text         | `string`    | The text on the button. |
| rect   | `Rectangle` | The rectangle describing its size.  |
| texture   | `Texture2D` | A 1x1 pixel texture that will be filled with the color gray.  |


------
## Class constructor

```cs
public UIButton(Rectangle rect, string buttonText, GraphicsDevice gDevice)
        {
            this.Rect = rect;
            this.Text = buttonText;
            this.texture = new Texture2D(gDevice, 1, 1);

            texture.SetData(new[] { Color.Gray });
        }
```


#### Input Arguments
 - `rect`: The position and size of the button descriebd in a `Rectangle`.
 - `buttonText`: The text on the button.
 - `gDevice`: The current graphicsdevice.

#### Description
This constructor sets the rectangle and text of the button, then uses gDevice to create a 1x1 pixel texture which it then fills with the color gray.  
This texture is stretched to the proper dimensions in the Draw function.  



------

## Properties

- Text
- Rect


### Text
```cs
public String Text
```

#### Set
Sets its value to text.

#### Get
Returns the value of text.

------
### Rect
```cs
 public Rectangle Rect
```

#### Set
Sets its value to rect.

#### Get
Returns the value of rect.


------

## Methods
```cs
public void Update(MouseState mouseState)
public void Draw(SpriteBatch spriteBatch)
```

### Update
```cs
public void Update(MouseState mouseState)
```

#### Input Arguments
- `mouseState`: The current `MouseState`

#### Description
Whenever this function is called, it checks whether the mouse is currently on the button or not.  
If it is, it will change the color of the button to a lighter one so that the button is highlighted.  
If it doesn't contain the position of the mouse it will revert to the default color.  


------
### Draw
```cs
public void Draw(SpriteBatch spriteBatch)
```

#### Input Arguments
- `spriteBatch`: The SpriteBatch

#### Description
The Draw method draws the 1x1 pixel texture onto `rect` by stretching it.  
Buttons are drawn on a layer depth of `0.2f`.

