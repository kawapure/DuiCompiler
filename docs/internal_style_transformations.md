# Internal Documentation :: Style Transformations

If you've ever looked at DirectUI files from Windows XP, you might have noticed that they handle styles in a very CSS-like language. This was simply an alternative syntax for stylesheets, and probably what Microsoft continued using internally.

DirectUI in Windows XP used a cheap parser they generated with [Bison](https://en.wikipedia.org/wiki/GNU_Bison). Things were pretty rigid back then, and I suppose it didn't hurt them to just throw together some fast XML and CSS clone to get some browser-like markup for writing desktop applications.

In Windows Vista, they rewrote the DUIXML parser. I imagine, whatever optimisations they were hoping to obtain from this, they didn't want to sacrifice by having to handle a completely different syntax just for stylesheets. Alas, they probably just wrote some tool to transform the styles from the CSS-like syntax to the XML node tree ahead-of-time to make parsing more efficient. It would only make sense, since CSS styles are way easier to read and they translate 1:1.

In XP-era parser terminology, the CSS-like language has no name—it is simply part of DUIXML—but certain parts do have explicit names (which mostly just line up with CSS terminology):
- `<style resid=""></style>` XML elements are called **sheets**.
- `element {}` blocks are called **rules**.
- `[attribute]`, `[attribute=value]`, and `[attribute != value]` conditions are called **attributes** or **conditions**, depending on the context.
- `property: value;` statements inside the rule blocks are called **declarations**.

## The simple explanation

```css
element
{
    property: value;
}
```

is the same as:

```xml
<element property=value />
```

and:

```css

element [condition]
{
    property: value;
}

```

is the same as:

```xml
<if condition="true">
    <element property=value />
</if>
```

## The technical explanation

DirectUI CSS syntax allows for, per rule:
- one element name to select (e.g. `Element`)
- zero or more attribute conditionals (e.g. `[mousefocused]` or `[visible]`)
    - which may contain a simple equation check operator for value comparison (`=` or `!=`)
- zero or more property declarations to apply all elements of the selected element name who pass the attribute conditions

Attribute conditionals control the entire behaviour of the set, so they are the most important part. In translation from the CSS-like language to XML nodes, they are promoted to DUIXML `if` or `unless` elements which parent the rule. In particular:
- If the condition is simply an attribute name (e.g. `[visible]`), then it will be given an explicit true value (equivalent to e.g. `[visible=true]`), and will be transformed to an `if` node parenting the rule:
    - ```css
      element [visible] {}
      ```
      will become:
      ```xml
      <if visible="true">
          <element />
      </if>
      ```
- If the condition is an attribute name followed by a single equal sign, then the condition will be straightforwardly transformed to an equivalent `if` node parenting the rule:
    - ```css
      element [id=atom(header)] {}
      ```
      will become:
      ```xml
      <if id="atom(header)">
          <element />
      </if>
      ```
- If the condition is an attribute name followed by a `!=` (not equals sign) character sequence, then the condition will be straightforwardly transformed to an equivalent `unless` node parenting the rule:
    - ```css
      MyDocumentElement [minimized != true] {}
      ```
      will become:
      ```xml
      <unless minimized="true">
          <MyDocumentElement />
      </unless>
      ```
      
As of the introduction of DUI70 (DirectUI library version 8) in Windows 7, a few more attribute operators are supported, allowing relative comparison:
- If the condition is followed by a single `<` (less than sign) character, which itself is followed by a value, then the condition will be transformed into an equivalent `iflesser` node parenting the rule:
    - ```css
      MyDocumentElement [pageNumber < 2] {}
      ```
      will become:
      ```xml
      <iflesser pageNumber="2">
          <MyDocumentElement />
      </iflesser>
      ```
- If the condition is followed by a single `>` (greater than sign) character, which itself is followed by a value, then the condition will be transformed into an equivalent `ifgreater` node parenting the rule:
    - ```css
      MyDocumentElement [pageNumber > 1] {}
      ```
      will become:
      ```xml
      <ifgreater pageNumber="1">
          <MyDocumentElement />
      </ifgreater>
      ```
- If the condition is followed by a `<=` (less than or equal to) character sequence followed by a value, then the condition will be transformed into an equivalent `iflesserequal` node parenting the rule:
    - ```css
      MyDocumentElement [pageNumber <= 0] {}
      ```
      will become:
      ```xml
      <iflesserequal pageNumber="0">
          <MyDocumentElement />
      </iflesserequal>
      ```
- If the condition is followed by a `>=` (greater than or equal to) character sequence followed by a value, then the condition will be transformed into an equivalent `ifgreaterequal` node parenting the rule:
    - ```css
      MyDocumentElement [pageNumber >= 2] {}
      ```
      will become:
      ```xml
      <ifgreaterequal pageNumber="2">
          <MyDocumentElement />
      </ifgreaterequal>
      ```
        
Here is the simple table of conversions from operators into their equivalent node names:

| **Operator** | **Node**       | Supported since   |
|--------------|----------------|-------------------|
| `(none)`     | If             | Windows XP/Vista  |
| =            | If             | Windows XP/Vista  |
| !=           | Unless         | Windows XP/Vista  |
| <            | IfLesser       | Windows 7 (DUI 8) |
| >            | IfGreater      | Windows 7 (DUI 8) |
| <=           | IfLesserEqual  | Windows 7 (DUI 8) |
| >=           | IfGreaterEqual | Windows 7 (DUI 8) |

Please note that all conditional node names are case-insensitive, so you could write `IfGreaterEqual` for readability's sake.
      
The element name itself and all style property declarations for the rule are coalesced into a single element, where the tag name is the name of the element, and all attributes are style property declarations.
- ```css
  MyDocumentElement
  {
      background: argb(255, 30, 30, 30);
      foreground: argb(255, 255, 255, 255);
  }
  ```
  will become:
  ```xml
  <MyDocumentElement background="argb(255, 30, 30, 30)" foreground="argb(255, 255, 255, 255)" />
  ```
  

## Differences between DUI stylesheets and regular browser CSS

Just because the DUI stylesheet language shares similar syntax with CSS doesn't mean that it functions like what you may be accustomed to in your browser.

Without even regarding the differences in element style properties and acceptable values, one of the biggest differences by far is how selectors work.

CSS selectors in your browser perform hierarchical searches for elements. As such, you can select a `h1` element inside of a parent element which has the ID "doc-container" and apply your styles to elements that only meet that criteria:

```css
#doc-container h1
{
    font-size: 16px;
    color: red;
}
```

Styles in DirectUI have no such ability. Rules in DirectUI styles have been unable to, since their inception, detail hierarchical data when selecting an element. Therefore, you may only have **one element name** to select with an unlimited number of attribute conditions.

Put simply, you can have `element [attr1][attr2] {}` to match all elements of type `element` which have the attributes `attr1` and `attr2` set, but you **cannot** have something like `MyCustomElement HWNDElement {}` to select a `HWNDElement` inside of a `MyCustomElement`.