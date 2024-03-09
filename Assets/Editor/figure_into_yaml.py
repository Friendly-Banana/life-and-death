with open("figures.txt") as f:
    a = f.read()
a = a.split("\n\n")
d = """
  cells:"""

for f in a:
    b=""
    i=False
    for line in f.split("\n"):
        if not i and line and not line[0] in " *":
            b+="\n  m_Name: " + line
            i=True
            continue
        b += "\n  - array:"
        for c in line:
            b += "01" if c == "*" else "00"
    m=max(*b.split("\n"), key=lambda x:len(x) if x.startswith("\n  - array:") else 0 )
    print(m)
    e=[]
    for line in b.split("\n"):
        if line.startswith("\n  - array:"):
            e.append(line.ljust(m, "0"))
        else: e.append(line)
    print(b)


