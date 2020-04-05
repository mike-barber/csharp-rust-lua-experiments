use rlua::{Function, Lua, Result};
use custom_types::ArrContainer;

fn main() -> Result<()> {
    let lua = Lua::new();

    lua.context(|ctx| {
        let globals = ctx.globals();

        globals.set("a", 1)?;
        globals.set("b", 2)?;

        Ok(())
    })?;

    lua.context(|ctx| {
        let globals = ctx.globals();

        println!("{}", globals.get::<_, i32>("a")?);
        println!("{}", globals.get::<_, i32>("b")?);

        // directly evaluate from globals
        let res = ctx.load("a + b").eval::<i32>()?;
        println!("res {}", res);

        // create and use lua function
        ctx.load(
            r#"
            function add(x,y) 
                return x+y
            end
        "#,
        )
        .exec()?;
        let add_func: Function = globals.get("add")?;
        let res2: i32 = add_func.call((10, 15))?;
        println!("res2 {}", res2);

        let arr1 = ArrContainer(ndarray::arr1(&[1, 2, 3]));
        let arr2 = ArrContainer(ndarray::arr1(&[4, 5, 6]));
        globals.set("arr1", arr1)?;
        globals.set("arr2", arr2)?;
        let arr_add = ctx.load("arr1 + arr2").eval::<ArrContainer<i32>>()?;
        println!("add {:?}", arr_add);

        let arr_mul = ctx.load("arr1 * arr2").eval::<ArrContainer<i32>>()?;
        println!("mul {:?}", arr_mul);

        let arr_sum = ctx.load("arr1:sum()").eval::<i32>()?;
        println!("sum {}", arr_sum);

        println!("first item {}", ctx.load("arr1[1]").eval::<i32>()?);
        println!("third item {}", ctx.load("arr1[3]").eval::<i32>()?);

        Ok(())
    })?;

    Ok(())
}
