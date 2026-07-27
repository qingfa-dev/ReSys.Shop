/**
 * Generic debounce wrapper for event handlers / callbacks.
 * Usage: const { debounced } = useDebounce((val) => doSearch(val), 300)
 */
export function useDebounce<T extends (...args: never[]) => void>(fn: T, delayMs = 300) {
  let timer: ReturnType<typeof setTimeout> | undefined;

  const debounced = (...args: Parameters<T>) => {
    if (timer) clearTimeout(timer);
    timer = setTimeout(() => fn(...args), delayMs);
  };

  const cancel = () => {
    if (timer) clearTimeout(timer);
  };

  return { debounced, cancel };
}
