# Sprite.cs
The sprite class is the base class for all objects that require graphics.
This was done so doing all the basic operations such as loading a texture and defining positions didn't have to be written for all other classes.  


-----

## Class Fields
| Name          | Type   | Explanation   |
| :-----------  | :----- | :------------ |
| position   | `Vector2` | The position of the sprite          |
| texture   | `Texture2D` | The texture of the sprite     |
| filename   | `string` | The filename of the sprite     |
| size   | `Rectangle` | The size of the sprite     |
| scale   | `float` | The scale of the sprite |

------
## Class constructors

```cs
public Sprite()
        {
            position = Vector2.Zero;
            scale = 1.0f;
        }
        
public Sprite(Vector2 pos, GraphicsDevice gDevice)
        {
            position = pos;
            scale = gDevice.Viewport.Width / 1024;
        }
```

------
###  public Sprite
```cs
public Sprite()
```

#### Input Arguments
None

#### Description
This constructor generates a sprite at position (0,0) and with scale 1.  


------
###  public Sprite
```cs
public Sprite(Vector2 pos, GraphicsDevice gDevice)
```

#### Input Arguments
- `pos`: The position of the sprite
- `gDevice`: The current graphics device.

#### Description
Using the parameters of gDevice, the scale is calculated based on the relative difference to 1024x768.  


------

## Methods
```cs
public void LoadContent(string file, ContentManager cManager)
public void Update()
public void Draw(SpriteBatch spriteBatch)
```

### LoadContent
```cs
public void LoadContent(string file, ContentManager cManager)
```

#### Input Arguments
- `file`: The filename.
- `cManager`: The current content manager.

#### Description
This method loads the image into the texture and saves the filename in the class.  


------
### Update
```cs
 public void Update()
```

#### Input Arguments
None.

#### Description
This is empty and should be overwritten in inherited classes.  


------
### Draw
```cs
 public void Draw(SpriteBatch spriteBatch)
```

#### Input Arguments
- `spriteBatch`: The sprite batch.

#### Description
This basic method draws the sprite at `position` with its current `scale`.  
The sprite will be drawn in the very background as the layer depth is `0.999f`.
  
