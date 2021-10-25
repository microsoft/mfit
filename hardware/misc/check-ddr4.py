#!/usr/bin/env python

def load_pins():
    ret = {}
    with open("DIMM-DDR4-288pins.txt", "rt") as f:
        for l in f.readlines():
            line = l.replace('\n', '')
            #print(line)
            idx, pin_name = line.split(' ')
            assert(int(idx) not in ret)
            ret[int(idx)] = pin_name
    return ret

def list_pins(pins):
    for pin_idx in range(288):
        pin_num = pin_idx+1
        #print(pin_num)
        print(pins[pin_num])

def list_pins_datasheet(pins):
    #asf36c2gx72pz%20(1).pdf
    # page 1
    page_cnt = [36, 12]
    first_pin = [1, 25]
    for page_idx in range(len(page_cnt)):
        for pin_idx in range(page_cnt[page_idx]):
            pin_num = pin_idx+first_pin[page_idx]
            s = ''
            for col in range(8):
                this_pin_num = pin_num + col * page_cnt[page_idx]
                s += '%03d %16s|' % (this_pin_num, pins[this_pin_num])
                if col == 3:
                    s += '|'
            print(s)
        print('\n')

def get_direction_of_pin(pins, pin_num):
    #if pins[pin_num] == 'NC':
    #    return 'N'
    n = pins[pin_num]
    if pins[pin_num] in ['VSS', 'VDD', 'VTT', 'VPP', 'VREFCA']:
        return 'W'
    if n.startswith('ALERT'):
        return 'O'
    input_types = ['DQS', 'BG', 'A', 'CS', 'BA', 'CK', 'PARITY']
    for t in input_types:
        if n.startswith(t):
            return 'I'

    if n.startswith('DQ'):
            return 'B'

    if n.startswith('CB'):
            return 'B'
    return 'U'

def get_pin_type(pins, pin_num):
    n = pins[pin_num]
    if n.startswith('CK'):
        return 'C'
    if n.endswith('_n'):
        return 'I'
    return ''

def gen_eeschema_symbol(pins):
    ###X first_pin ~ 150 150 100 R 50 50 1 1 B
    ###X left_pin ~ -600 150 100 L 50 50 1 1 B
    ###X second_pin ~ 150 100 100 R 50 50 1 1 B
    # do two boxes, on front and one on the back
    # 1 - 72
    gap = 100

    # where to stat the columns of pins, left and right column
    X = [-2200, -800, 800, 2200]
    Y =  ((72-1) * gap) / 2

    for side in range(4):
        if side % 2:
            lr = 'L'
        else:
            lr = 'R'

        print("# start ")
        for idx in range(72):
            pin_num = 72 * side + idx+1
            s = 'X %s %d %d %d 200 %c 50 50 1 1 %s %s' % (pins[pin_num], pin_num, X[side], Y-gap * ((pin_num-1)% 72), lr, \
                    get_direction_of_pin(pins, pin_num), get_pin_type(pins, pin_num))
            print(s)
        print("# to %d done" % (72 * side +idx+1))

def gen_eeschema_labels(pins):
    ###X first_pin ~ 150 150 100 R 50 50 1 1 B
    ###X left_pin ~ -600 150 100 L 50 50 1 1 B
    ###X second_pin ~ 150 100 100 R 50 50 1 1 B
    # do two boxes, on front and one on the back
    # 1 - 72
    #gap = 100
    Y0 = 600
    gapY =  100

    # where to stat the columns of pins, left and right column
    X0 = [1200, 2600, 4200, 5600]

###Text GLabel 5400 800  2    50   Input ~ 0
###CK1_c_MB
    suffix = '_DIMM'

    for side in range(4):
        if side % 2:
            lr = '2'
        else:
            lr = '0'

        #print("# start ")
        for idx in range(72):
            pin_num = 72 * side + idx+1
            ##s = 'X %s %d %d %d 200 %c 50 50 1 1 %s %s' % (pins[pin_num], pin_num, X[side], Y-gap * ((pin_num-1)% 72), lr, \
            ##        get_direction_of_pin(pins, pin_num), get_pin_type(pins, pin_num))
            x = X0[side]
            y = Y0 + idx*gapY
            s = 'Text GLabel %d %d %s 50 Input ~ 0\n%s%s' % (x, y, lr, pins[pin_num].replace('/', '_'), suffix)
            ###Text GLabel 5400 800  2    50   Input ~ 0
            ###CK1_c_MB
            print(s)
        #print("# to %d done" % (72 * side +idx+1))

def should_auto_connect(pins, pin_num):
    n = pins[pin_num]

    if n == 'ALERT_n':
        return False

    return True

def gen_eeschema_one_to_one_conn(pins):

    #### Wire Wire Line
    #### 	800  700  950  700 
    #### Text GLabel 800  700  0    50   Input ~ 0
    #### NC_MB
    #### Text GLabel 950  700  2    50   Input ~ 0
    #### NC_DIMM
    Y0 = 600
    gapY =  100

    # where to stat the columns of pins, left and right column
    X0 = [1200, 2800, 5200, 6800]

    wire_len = 100

    for side in range(4):
        if side % 2:
            lr = '2'
        else:
            lr = '0'

        for idx in range(72):
            pin_num = 72 * side + idx+1
            ##s = 'X %s %d %d %d 200 %c 50 50 1 1 %s %s' % (pins[pin_num], pin_num, X[side], Y-gap * ((pin_num-1)% 72), lr, \
            ##        get_direction_of_pin(pins, pin_num), get_pin_type(pins, pin_num))
            x = X0[side]

            y = Y0 + idx*gapY

            s = ''

            if should_auto_connect(pins, pin_num):
                s += 'Wire Wire Line\n\t%d %d %d %d\n' % (x, y, x+wire_len, y)

            suffix = '_MB'
            s += 'Text GLabel %d %d %s 50 Input ~ 0\n%s%s\n' % (x, y, '0', pins[pin_num].replace('/', '_'), suffix)

            suffix = '_DIMM'
            x += wire_len
            s += 'Text GLabel %d %d %s 50 Input ~ 0\n%s%s\n' % (x, y, '2', pins[pin_num].replace('/', '_'), suffix)
            print(s)

def gen_footprint_conn_ddr4_288_sm():
    # adex electronics
    #  (pad 1 smd rect (at -64.6 9.91) (size 0.5 2) (layers F.Cu F.Mask))
    #  (pad 145 smd roundrect (at -64.6 9.91 180) (size 0.6 2.6) (layers B.Cu B.Mask) (roundrect_rratio 0.25))
    X0 = -64.0
    Y0 = 10.0
    for pin in range(144):
        pad_front = pin + 1
        pad_back  = pin + 1 + 144
        gap = 0
        if pin > 76:
            gap = 5.95-0.85
        x = X0 + pin * 0.85 + gap
        y = Y0
        front = "  (pad %d smd rect (at %f %f) (size 0.5 2) (layers F.Cu F.Mask))" % (pad_front, x, y)
        back = "  (pad %d smd rect (at %f %f 180) (size 0.5 2) (layers B.Cu B.Mask))" % (pad_back, x, y)
        print(front)
        print(back)


pins = load_pins()
list_pins_datasheet(pins)
##gen_eeschema_symbol(pins)
###gen_eeschema_labels(pins)
##gen_eeschema_one_to_one_conn(pins)
##gen_footprint_conn_ddr4_288_sm()
