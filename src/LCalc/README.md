# LCalc

A fast and efficient string calculator  
**Usage**  
lcalc {expression}  
**Description**  
Calculate an expression with optional arguments  
**Arguments**  
`&{name}={value}`  Set a variable/argument's value  
`&{name}`  Set calculator argument  
Arguments:
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
> Custom function: `[{functionName}([arg] [arg...])={body}]`  
> `abs({value})` Absolute value  
> `clamp({value})` `ceiling({value})` `round({value} [digits])` Rounding numbers  
> `cos({deg})` `sin({deg})` `tan({deg})` `cot({deg})` Trigonometric  
> `log({value})` Logarithm  
> `sqrt({value})` `cbrt({value})` Square root, cube root  
> `sum({value} [values..])` `avg({value} [values..])` Sum, average  
> `random([max|min,max])` Get random number in ranges: 0-1, 0-max, min-max

**__Special number__**
> `0b{binaryNum}`  Binary number  
> `0x{hexNum}`  Hexadecimal number  
> `0o{octalNum}`  Octal number  
