# LCalc

```
A fast and efficient string calculator

-- Usage --
Calculator.CalcFormatted()   Calculate and format the output  
Calculator.CalcRaw()         Calculate the expression the return the raw result
```

## Syntax

`[custom functions..] {expression} [args..]`

- `|`  or
- `()`  group
- `[]`  optional
- `{}`  value

> **__Custom function__**  
> `[{name}([args..])(=| ){expression}]`  
> <h6>The square brackets around the function is required<br />Call the function by using `name()` just like normal
> functions</h6>
> `name` The name of the custom function  
> `expression` The expression  
> `args` The arguments

> **__Arguments__**  
> `&{name}={number}`  Set a variable's value  
> `&{name}` or `&{name}={text}`  Set calculator argument  
> `&step` Add solving steps  
> `&tree` Like &step but wrap every node with a `()`  
> `&raw` See raw value  
> `&solve` Solve for the unknown variable so that the result is 0  
> `&render` Render the expression  
> `&latex` Add solving steps in LaTeX syntax  
> `&latexdoc` Add solving steps in LaTeX syntax and wrap everything in a LaTeX document  
> `nolatexdoc` Allow &latex but not &latexdoc (Set through code)
> ```
> &fmt={format} or &format={format}  Change output result format
> 
> format:                            Default format is human
> human                              Easy to read format
> none|raw                           No format
> hex                                Format in hex
> octal|oct                          Format in octal
> binary|bin                         Format in binary
> ```

> **__Comparison operators__**  
> `==` Equal  
> `!=` Not equal  
> `>=` Greater than or equal  
> `<=` Less than or equal  
> `>` Greater than  
> `<` Less than

> **__Calculation operators__**
> ```
>  +   -   *   /   ^   %   !     Standard operators  
> add sub mul div pow mod fac  
> ||  &  ^^   ~   >>   <<        Bitwise operators  
> or and xor not rshf lshr
> ```

> **__Functions__**  
> Use: `functionName([arg][,] [arg...])`  
> `abs({value})` `|{value}|` Absolute value  
> `clamp({value})` `ceiling({value})` `round({value} [digits])` Rounding numbers  
> `cos({deg})` `sin({deg})` `tan({deg})` `cot({deg})` Trigonometric  
> `log({value})` Logarithm  
> `sqrt({value})` `cbrt({value})` Square root, cube root  
> `sum({value} [values..])` `avg({value} [values..])` Sum, average  
> `random([max|(min,max)])` Get random number in ranges: 0-1, 0-max, min-max  
> `sigma({arg}, {start}, {end}, {expression})`: Sigma (Σ)  
> `cpi({arg}, {start}, {end}, {expression})`: Capital Pi (Π)

> **__Special number__**  
> `0b{binary}`  Binary number  
> `0x{hex}`  Hexadecimal number  
> `0o{octal}`  Octal number  
