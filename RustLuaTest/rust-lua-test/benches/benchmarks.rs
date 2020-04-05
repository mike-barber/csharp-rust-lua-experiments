use criterion::*;
use ndarray::{arr1, Array1};
use rlua::{Lua, Function};
use custom_types::ArrContainer;

fn get_a1() -> Array1<i32> {
    arr1(&[1, 2, 3, 4, 5, 6, 7, 8, 9, 10])
}

fn get_a2() -> Array1<i32> {
    arr1(&[11, 12, 13, 14, 15, 16, 17, 18, 19, 20])
}

fn bench_ndarray_multiply(b: &mut Criterion) {
    let a1 = get_a1();
    let a2 = get_a2();
    b.bench_function("ndarray", |b| {
        b.iter(|| {
            let res = &a1 * &a2;
            res
        })
    });
}

fn bench_ndarray_multiply_clone(b: &mut Criterion) {
    let a1 = get_a1();
    let a2 = get_a2();
    b.bench_function("ndarray", |b| {
        b.iter(|| {
            let res = a1.clone() * a2.clone();
            res
        })
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

                let arr_mul = ctx.load("arr1 * arr2").eval::<ArrContainer<i32>>()?;
                assert!(arr_mul.0[[0]] == 11);

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
        ctx.load(r#"
        function mult(a,b) 
            return a * b
        end
        "#).exec()?;
        
        rlua::Result::<()>::Ok(())
    }).unwrap();

    b.bench_function("lua multiply / function call", |b| {
        b.iter(|| {
            lua.context(|ctx| {
                let globals = ctx.globals();
                let mult: Function = globals.get("mult")?;

                let arr_mul: ArrContainer<i32> = mult.call((arr1.clone(), arr2.clone()))?;
                assert!(arr_mul.0[[0]] == 11);

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
