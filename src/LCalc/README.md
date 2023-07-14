# LCalc

A fast and efficient string calculator  
**Usage**  
Calculator.CalcFormatted() calculate and format the output  
Calculator.CalcRaw() calculate the expression the return the raw result  
**Description**  
Calculate an expression with optional arguments  
**Syntax**  
`[custom functions..] {expression} [args..]`  
<h6>`|`: or - `[]`: optional - `{}`: value</h6>
> Custom function:  
> `[{name}([args..])(=| ){expression}]`  
> <h6>The square brackets around the function is required<br />Call the function by using `name()` just like normal
> functions</h6>
> `name` The name of the custom function  
> `expression` The expression  
> `args` The arguments
>
> Arguments:  
> `&{name}={value}`  Set a variable's value  
> `&{name}`  Set calculator argument  
> `step` Show each step  
> `tree` Like &step but wrap every node with a `()`  
> `raw` See raw value  
> `solve` Solve for the unknown variable so that the result is 0

**__Comparison operators__**
> `==` Equal  
> `!=` Not equal  
> `>=` Greater than or equal  
> `<=` Less than or equal  
> `>` Greater than  
> `<` Less than
>
**__Calculation operators__**
> `+` `-` `*` `/` `^` `%` `!` Standard operators  
> `|` `&` `^^` `~` `>>` `<<` Bitwise operators
>
**__Functions__**
> Use: `functionName([arg][,] [arg...])`  
> `abs({value})` Absolute value  
> `clamp({value})` `ceiling({value})` `round({value} [digits])` Rounding numbers  
> `cos({deg})` `sin({deg})` `tan({deg})` `cot({deg})` Trigonometric  
> `log({value})` Logarithm  
> `sqrt({value})` `cbrt({value})` Square root, cube root  
> `sum({value} [values..])` `avg({value} [values..])` Sum, average  
> `random([max|(min,max)])` Get random number in ranges: 0-1, 0-max, min-max
> `sigma({arg}, {start}, {end}, {expression})`: Sigma (Σ)  
> `cpi({arg}, {start}, {end}, {expression})`: Capital Pi (Π)

**__Special number__**
> `0b{binary}`  Binary number  
> `0x{hex}`  Hexadecimal number  
> `0o{octal}`  Octal number  
