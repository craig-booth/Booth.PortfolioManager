import { Dispatch, SetStateAction, useEffect, useState } from "react";

export function useLocalStorage<T>(key: string, initialValue: T) : [T, Dispatch<SetStateAction<T>>] {

    const [storedValue, setStoredValue] = useState(initialValue);

    const setValue: Dispatch<SetStateAction<T>> = (value) => {
        
        window.localStorage.setItem(key, JSON.stringify(value));
        setStoredValue(value);
    }

    useEffect(() => {
        const rawValue = window.localStorage.getItem(key);
        const value = rawValue ? JSON.parse(rawValue): initialValue;

        setStoredValue(value);
    // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [key])

    return [storedValue, setValue];
}