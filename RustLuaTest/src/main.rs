use ndarray::Array1;
use rlua::{Function, Lua, MetaMethod, Result, UserData, UserDataMethods};

#[derive(Clone, Debug)]
struct ArrContainer<T>(Array1<T>);

// should probably look at implementing a macro to support other types
impl UserData for ArrContainer<i32> {
    fn add_methods<'lua, M: UserDataMethods<'lua, Self>>(methods: &mut M) {
        methods.add_method("sum", |_, vec, ()| {
            let sum = vec.0.sum();
            Ok(sum)
        });

        methods.add_meta_function(
            MetaMethod::Add,
            |_, (v1, v2): (ArrContainer<i32>, ArrContainer<i32>)| {
                let s = v1.0 + v2.0;
                Ok(ArrContainer(s))
            },
        );

        methods.add_meta_function(
            MetaMethod::Mul,
            |_, (v1, v2): (ArrContainer<i32>, ArrContainer<i32>)| {
                let s = v1.0 * v2.0;
                Ok(ArrContainer(s))
            },
        );
    }
}

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

        Ok(())
    })?;

    Ok(())
}
