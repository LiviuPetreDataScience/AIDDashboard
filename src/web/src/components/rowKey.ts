/**
 * Assigns a stable unique key to each row object for AG Grid's row identity. List-style
 * rows have no natural key (new rows share id 0), so we map each object instance to a
 * generated key that stays stable for that object's lifetime.
 */
const objectKeys = new WeakMap<object, string>();
let counter = 0;

export function stableRowKey(row: object): string {
  let key = objectKeys.get(row);
  if (!key) {
    key = `row-${counter++}`;
    objectKeys.set(row, key);
  }
  return key;
}
