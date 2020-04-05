use ndarray::Array1;
use rlua::{Error, MetaMethod, UserData, UserDataMethods};
use std::convert::TryFrom;

#[derive(Clone, Debug)]
pub struct ArrContainer<T>(pub Array1<T>);

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

        methods.add_meta_function(
            MetaMethod::Index,
            |_, (arr, idx): (ArrContainer<i32>, i32)| {
                let a = &arr.0;
                
                // change to zero-indexed, and convert to usize; return an error
                // if the conversion fails (which it will do for negative values)
                let i = usize::try_from(idx - 1)
                    .map_err(|_| Error::RuntimeError("invalid index".to_owned()))?;

                // check we're within range
                if i >= a.shape()[0] {
                    return Err(Error::RuntimeError("Invalid index".to_owned()));
                }

                // return the value
                Ok(a[[i]])
            },
        );
    }
}


#[cfg(test)]
mod tests {
    #[test]
    fn it_works() {
        assert_eq!(2 + 2, 4);
    }
}
