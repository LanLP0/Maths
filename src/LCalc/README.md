# LCalc

A fast and efficient string calculator  
**Usage**  
lcalc {expression}  
**Description**  
Calculate an expression with optional arguments  
**Arguments**  
`&{name}={value}`  Set a variable's value  
`&{name}`  Set calculator argument  
Arguments:
> `step` Show each step  
> `raw` See raw value

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
> Use: `functionName([arguments])`  
> Custom function: `[{functionName}([arg...])={body}]`  
> `abs({value})` Absolute value  
> `clamp({value})` `ceiling({value})` `round({value} [digits])` Rounding numbers  
> `cos({deg})` `sin({deg})` `tan({deg})` `cot({deg})` Trigonometric  
> `log({value})` Logarithm  
> `sqrt({value})` `cbrt({value})` Square root, cube root  
> `sum({value} [values..])` `avg({value} [values..])` Sum, average  
> `random([max|min,max])` Get random number in ranges: 0-1, 0-max, min-max  
**__Special number__**  
> `&b{binaryNum}`  Binary number  
> `&h{hexNum}`  Hexadecimal number  
> `&o{octalNum}`  Octal number  
