use criterion::*;
use custom_types::ArrContainer;
use ndarray::{arr1, Array1};
use rlua::{Function, Lua};

fn get_a1() -> Array1<i32> {
    arr1(&(0..100).collect::<std::vec::Vec<_>>())
}

fn get_a2() -> Array1<i32> {
    arr1(&(100..200).collect::<std::vec::Vec<_>>())
}

fn bench_ndarray_multiply(b: &mut Criterion) {
    let a1 = get_a1();
    let a2 = get_a2();
    b.bench_function("ndarray mult", |b| b.iter(|| &a1 * &a2));
}

fn bench_ndarray_multiply_clone(b: &mut Criterion) {
    let a1 = get_a1();
    let a2 = get_a2();
    b.bench_function("ndarray mult / clone", |b| {
        b.iter(|| a1.clone() * a2.clone())
    });
}

fn bench_lua_setup(b: &mut Criterion) {
    b.bench_function("lua setup", |b| {
        b.iter(|| {
            let lua = Lua::new();
            lua.context(|ctx| {
                let globals = ctx.globals();
                globals.set("a", 1)?;
                globals.set("b", 2)?;
                rlua::Result::<()>::Ok(())
            })
            .unwrap();
        })
    });
}

fn bench_lua_multiply(b: &mut Criterion) {
    let lua = Lua::new();
    let arr1 = ArrContainer(get_a1());
    let arr2 = ArrContainer(get_a2());
    b.bench_function("lua multiply / globals", |b| {
        b.iter(|| {
            lua.context(|ctx| {
                let globals = ctx.globals();

                globals.set("arr1", arr1.clone())?;
                globals.set("arr2", arr2.clone())?;

                let _ = ctx.load("arr1 * arr2").eval::<ArrContainer<i32>>()?;

                rlua::Result::<()>::Ok(())
            })
            .unwrap();
        })
    });
}

fn bench_lua_multiply_call(b: &mut Criterion) {
    let lua = Lua::new();
    let arr1 = ArrContainer(get_a1());
    let arr2 = ArrContainer(get_a2());

    lua.context(|ctx| {
        ctx.load(
            r#"
        function mult(a,b) 
            return a * b
        end
        "#,
        )
        .exec()?;
        rlua::Result::<()>::Ok(())
    })
    .unwrap();

    b.bench_function("lua multiply / function call", |b| {
        b.iter(|| {
            lua.context(|ctx| {
                let globals = ctx.globals();
                let mult: Function = globals.get("mult")?;

                let _: ArrContainer<i32> = mult.call((arr1.clone(), arr2.clone()))?;

                rlua::Result::<()>::Ok(())
            })
            .unwrap();
        })
    });
}

criterion_group!(
    benches,
    bench_ndarray_multiply,
    bench_ndarray_multiply_clone,
    bench_lua_setup,
    bench_lua_multiply,
    bench_lua_multiply_call
);
criterion_main!(benches);
