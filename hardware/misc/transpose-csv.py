with open('../fault-injector-sim/simulate-inj.csv') as file:
    lis = [x.replace('\n', '').split(';') for x in file]

for x in zip(*lis):
    for y in x:
        print(y + ', ', end="")
    print('')
