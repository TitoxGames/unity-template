# TitoX Custom Inspector System

A powerful custom inspector system inspired by Odin Inspector, providing a comprehensive set of attributes to enhance Unity's default inspector.

## 🎯 **Key Feature: Automatic Build Optimization**

**All attributes are automatically excluded from builds!** No manual `#if UNITY_EDITOR` directives needed. Simply use the attributes normally and they'll be automatically removed during build compilation.

## Architecture

The system uses a single file with conditional compilation to provide a clean API:

### **All Attributes (Automatically Editor-Only)**
- **[ShowInInspector]** - Show private fields in inspector
- **[HideInInspector]** - Hide public fields from inspector
- **[ShowIf]** - Show field only when condition is met
- **[HideIf]** - Hide field only when condition is met
- **[ReadOnly]** - Make fields read-only in inspector
- **[Button]** - Create buttons that call methods
- **[CustomHeader]** - Add headers with different styles (avoids conflict with Unity's Header)
- **[InfoBox]** - Display information boxes with different types
- **[PropertySpace]** - Add spacing between properties
- **[PropertyOrder]** - Control property order in inspector
- **[ValidateInput]** - Validate field values using methods
- **[PropertyRange]** - Add range sliders to numeric fields

## Performance Benefits

- **Automatic Build Optimization**: All attributes are automatically excluded from builds using conditional compilation
- **Clean API**: No manual `#if UNITY_EDITOR` directives required
- **Runtime Efficiency**: Zero performance impact in final builds
- **Memory Savings**: No inspector code in final builds
- **Compilation Speed**: Faster builds due to reduced code size

## Features

### Basic Attributes

```csharp
public class Example : MonoBehaviour
{
    [ShowInInspector]
    private string privateField = "This is shown";
    
    [HideInInspector]
    public string publicField = "This is hidden";
    
    [ReadOnly]
    public float readOnlyValue = 42f;
}
```

### Conditional Display

```csharp
public class Example : MonoBehaviour
{
    public bool showAdvancedSettings = false;
    
    [ShowIf("showAdvancedSettings")]
    public float advancedValue = 10f;
    
    [HideIf("showAdvancedSettings")]
    public string simpleMessage = "Settings hidden";
    
    // Show when value equals specific value
    [ShowIf("showAdvancedSettings", true)]
    public string conditionalField = "Only when true";
    
    // Invert condition
    [ShowIf("showAdvancedSettings", false)]
    public string invertedField = "Only when false";
}
```

### Visual Enhancement

```csharp
public class Example : MonoBehaviour
{
    [PropertySpace(20f)]
    [CustomHeader("Advanced Settings", HeaderStyle.Bold)]
    [InfoBox("This is a warning!", InfoBoxType.Warning)]
    [PropertyRange(0f, 100f)]
    [ValidateInput("ValidatePositive", "Value must be positive!", ValidationType.Error)]
    public float advancedValue = 50f;
}
```

### Buttons

```csharp
public class Example : MonoBehaviour
{
    [Button("Reset Values", ButtonStyle.Primary)]
    public void ResetValues()
    {
        // Reset logic here
        Debug.Log("Reset!");
    }
    
    [Button("Danger Action", ButtonStyle.Danger)]
    public void DeleteObject()
    {
        Debug.LogWarning("Deleted!");
    }
}
```

## Usage Examples

### Basic Attributes

```csharp
public class Example : MonoBehaviour
{
    [ShowInInspector]
    private string privateField = "This is shown";
    
    [HideInInspector]
    public string publicField = "This is hidden";
    
    [ReadOnly]
    public float readOnlyValue = 42f;
}
```

### Conditional Display

```csharp
public class Example : MonoBehaviour
{
    public bool showAdvancedSettings = false;
    
    [ShowIf("showAdvancedSettings")]
    public float advancedValue = 10f;
    
    [HideIf("showAdvancedSettings")]
    public string simpleMessage = "Settings hidden";
    
    // Show when value equals specific value
    [ShowIf("showAdvancedSettings", true)]
    public string conditionalField = "Only when true";
    
    // Invert condition
    [ShowIf("showAdvancedSettings", false)]
    public string invertedField = "Only when false";
}
```

### Headers and Spacing

```csharp
public class Example : MonoBehaviour
{
    [CustomHeader("Basic Settings")]
    public string basicField = "Basic";
    
    [PropertySpace(20f)]
    [CustomHeader("Advanced Settings", HeaderStyle.Bold)]
    public string advancedField = "Advanced";
    
    [CustomHeader("Colored Header", HeaderStyle.Colored)]
    public string coloredField = "Colored";
    
    [CustomHeader("Separator Header", HeaderStyle.Separator)]
    public string separatorField = "Separator";
}
```

### Info Boxes

```csharp
public class Example : MonoBehaviour
{
    [InfoBox("This is an info message", InfoBoxType.Info)]
    public string infoField = "Info";
    
    [InfoBox("This is a warning", InfoBoxType.Warning)]
    public string warningField = "Warning";
    
    [InfoBox("This is an error", InfoBoxType.Error)]
    public string errorField = "Error";
    
    [InfoBox("This is a success message", InfoBoxType.Success)]
    public string successField = "Success";
    
    // Conditional info box
    [InfoBox("Only shows when condition is true", "showAdvancedSettings", true)]
    public string conditionalInfoField = "Conditional";
}
```

### Validation

```csharp
public class Example : MonoBehaviour
{
    [ValidateInput("ValidatePositiveValue", "Value must be positive!", ValidationType.Error)]
    public float positiveValue = 5f;
    
    [ValidateInput("ValidateStringLength", "String too short", ValidationType.Warning)]
    public string validatedString = "Hello";
    
    // Validation methods (available at runtime)
    private bool ValidatePositiveValue(float value)
    {
        return value > 0f;
    }
    
    private bool ValidateStringLength(string value)
    {
        return !string.IsNullOrEmpty(value) && value.Length >= 3;
    }
}
```

### Range Sliders

```csharp
public class Example : MonoBehaviour
{
    [PropertyRange(0f, 100f)]
    public float health = 50f;
    
    [PropertyRange(0, 10)]
    public int level = 5;
    
    // Dynamic range from other fields
    public float minValue = 0f;
    public float maxValue = 100f;
    
    [PropertyRange("minValue", "maxValue")]
    public float dynamicRangeValue = 50f;
}
```

### Property Ordering

```csharp
public class Example : MonoBehaviour
{
    [PropertyOrder(3)]
    public string thirdField = "Third";
    
    [PropertyOrder(1)]
    public string firstField = "First";
    
    [PropertyOrder(2)]
    public string secondField = "Second";
}
```

### Buttons

```csharp
public class Example : MonoBehaviour
{
    [Button("Reset Values", ButtonStyle.Primary)]
    public void ResetValues()
    {
        // Reset logic here
        Debug.Log("Reset!");
    }
    
    [Button("Log Values", ButtonStyle.Secondary)]
    public void LogValues()
    {
        Debug.Log("Logging values...");
    }
    
    [Button("Success Action", ButtonStyle.Success)]
    public void SuccessAction()
    {
        Debug.Log("Success!");
    }
    
    [Button("Warning Action", ButtonStyle.Warning)]
    public void WarningAction()
    {
        Debug.LogWarning("Warning!");
    }
    
    [Button("Danger Action", ButtonStyle.Danger)]
    public void DangerAction()
    {
        Debug.LogError("Danger!");
    }
    
    // Button with method name as text
    [Button]
    public void SimpleButton()
    {
        Debug.Log("Simple button clicked!");
    }
}
```

## Advanced Examples

### Complex Conditional Display

```csharp
public class AdvancedExample : MonoBehaviour
{
    public enum GameMode { Easy, Normal, Hard }
    
    public GameMode currentMode = GameMode.Normal;
    public bool enableCheats = false;
    
    [ShowIf("currentMode", GameMode.Hard)]
    public float hardModeMultiplier = 2f;
    
    [ShowIf("enableCheats")]
    [InfoBox("Cheats are enabled!", InfoBoxType.Warning)]
    public string cheatCode = "GODMODE";
    
    [ShowIf("enableCheats")]
    [Button("Activate Cheat", ButtonStyle.Danger)]
    public void ActivateCheat()
    {
        Debug.Log("Cheat activated!");
    }
}
```

### Validation with Multiple Conditions

```csharp
public class ValidationExample : MonoBehaviour
{
    [ValidateInput("ValidateHealth", "Health must be between 0 and 100", ValidationType.Error)]
    public float health = 50f;
    
    [ValidateInput("ValidateName", "Name must be 3-20 characters", ValidationType.Warning)]
    public string playerName = "Player";
    
    [ValidateInput("ValidateEmail", "Invalid email format", ValidationType.Error)]
    public string email = "player@example.com";
    
    private bool ValidateHealth(float value)
    {
        return value >= 0f && value <= 100f;
    }
    
    private bool ValidateName(string value)
    {
        return !string.IsNullOrEmpty(value) && value.Length >= 3 && value.Length <= 20;
    }
    
    private bool ValidateEmail(string value)
    {
        return !string.IsNullOrEmpty(value) && value.Contains("@") && value.Contains(".");
    }
}
```

### Dynamic UI with Buttons

```csharp
public class DynamicUIExample : MonoBehaviour
{
    [CustomHeader("Dynamic Controls")]
    public bool showControls = true;
    
    [ShowIf("showControls")]
    [Button("Generate Random Color", ButtonStyle.Primary)]
    public void GenerateRandomColor()
    {
        GetComponent<Renderer>().material.color = Random.ColorHSV();
    }
    
    [ShowIf("showControls")]
    [Button("Reset Color", ButtonStyle.Secondary)]
    public void ResetColor()
    {
        GetComponent<Renderer>().material.color = Color.white;
    }
    
    [ShowIf("showControls")]
    [Button("Toggle Controls", ButtonStyle.Success)]
    public void ToggleControls()
    {
        showControls = !showControls;
    }
}
```

## Best Practices

### 1. Performance
- Use validation methods sparingly for complex calculations
- Cache reflection results when possible
- Keep validation methods simple and fast
- **No manual #if UNITY_EDITOR needed!**

### 2. Organization
- Use headers to group related properties
- Use property ordering for logical flow
- Use spacing to improve readability
- All attributes are automatically editor-only

### 3. Validation
- Provide clear error messages
- Use appropriate validation types (Error, Warning, Info)
- Keep validation logic simple and focused
- Validation methods are available at runtime

### 4. Conditional Display
- Use meaningful condition field names
- Consider using enums for complex conditions
- Test all condition combinations
- Conditional logic works in editor only

### 5. Buttons
- Use appropriate button styles for actions
- Provide clear button text
- Handle exceptions in button methods
- Buttons are automatically editor-only

## Build Optimization

### Automatic Code Removal
- **All attributes are automatically excluded from builds**
- No performance impact in final builds
- Reduced build size and compilation time
- Clean separation of concerns

### Runtime Safety
- Zero inspector code in final builds
- No reflection overhead at runtime
- No editor dependencies
- Clean runtime performance

## Troubleshooting

### Common Issues

1. **Attributes not working:**
   - Ensure the script is in the Editor folder
   - Check that the attribute is applied correctly
   - Verify field names in conditional attributes
   - **No manual #if UNITY_EDITOR needed!**

2. **Validation not working:**
   - Ensure validation method exists and is accessible
   - Check method signature (should return bool)
   - Verify parameter types match field type
   - Validation methods work at runtime

3. **Buttons not appearing:**
   - Ensure method is public or has Button attribute
   - Check that method has no parameters
   - Verify method is not static
   - Buttons are automatically editor-only

4. **Conditional display not working:**
   - Check field names are correct
   - Ensure condition field exists and is accessible
   - Verify condition values match field types
   - Conditional logic works in editor only

5. **Build errors:**
   - **All attributes are automatically excluded from builds**
   - No manual conditional compilation needed
   - Clean build process

### Debug Tips

1. Use Debug.Log in validation methods to check values
2. Test conditional logic with simple boolean fields first
3. Check Unity console for error messages
4. Use the example script as a reference
5. **Builds are automatically clean - no manual work needed**

## Extending the System

The system is designed to be extensible. You can:

1. **Add new attributes** by adding them to the runtime attributes file with `#if UNITY_EDITOR`
2. **Extend property drawers** by modifying CustomPropertyDrawer
3. **Add new button styles** by extending ButtonStyle enum
4. **Create custom editors** for specific component types
5. **All new attributes are automatically editor-only**

## File Structure

```
Assets/TitoX/Core/
├── Runtime/
│   └── Attributes/
│       └── InspectorAttributes.cs (All attributes with #if UNITY_EDITOR)
└── Editor/
    ├── PropertyDrawers/
    ├── Editors/
    ├── Examples/
    └── README.md
```

## 🎉 **Key Benefits**

- **Zero Manual Work**: No `#if UNITY_EDITOR` directives needed
- **Automatic Optimization**: All attributes excluded from builds automatically
- **Clean API**: Use attributes normally without worrying about build issues
- **Performance**: Zero runtime overhead in final builds
- **Maintainability**: Single source of truth for all attributes
- **No Conflicts**: CustomHeaderAttribute avoids conflicts with Unity's built-in HeaderAttribute

## License

This custom inspector system is part of the TitoX framework and follows the same licensing terms. 