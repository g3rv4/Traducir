const nameofFactory = <T>() => (name: keyof T) => name;

export default nameofFactory;
