import os

file_path = r'c:\Users\ASUS\Documents\Unity\Game-Unity-C-\Assets\Scenes\00_MainMenu\UI.unity'

with open(file_path, 'r', encoding='utf-8') as f:
    lines = f.readlines()

new_lines = []
i = 0
while i < len(lines):
    if lines[i].startswith('<<<<<<< HEAD'):
        if i < 700:
            # We are in the C1-C3 cluster.
            # Let's collect everything from here until the end of C3.
            # We know C3 ends with >>>>>>> 
            start_i = i
            c_blocks = []
            
            # We will parse C1, S1, C2, S2, C3
            # We just manually group them!
            # Let's find end of C3
            end_i = i
            conflict_count = 0
            while end_i < len(lines):
                if lines[end_i].startswith('>>>>>>>'):
                    conflict_count += 1
                    if conflict_count == 3:
                        break
                end_i += 1
            
            cluster = lines[start_i : end_i + 1]
            
            # Now parse the cluster into H1, I1, S1, H2, I2, S2, H3, I3
            # cluster is a list of lines
            idx = 0
            
            def get_conflict(idx, cluster):
                # skip <<<<<<< HEAD
                idx += 1
                head = []
                while not cluster[idx].startswith('======='):
                    head.append(cluster[idx])
                    idx += 1
                # skip =======
                idx += 1
                inc = []
                while not cluster[idx].startswith('>>>>>>>'):
                    inc.append(cluster[idx])
                    idx += 1
                # skip >>>>>>>
                idx += 1
                return head, inc, idx

            H1, I1, idx = get_conflict(idx, cluster)
            
            S1 = []
            while not cluster[idx].startswith('<<<<<<< HEAD'):
                S1.append(cluster[idx])
                idx += 1
                
            H2, I2, idx = get_conflict(idx, cluster)
            
            S2 = []
            while not cluster[idx].startswith('<<<<<<< HEAD'):
                S2.append(cluster[idx])
                idx += 1
                
            H3, I3, idx = get_conflict(idx, cluster)
            
            # Now construct combined
            new_lines.extend(H1)
            new_lines.extend(S1)
            new_lines.extend(H2)
            new_lines.extend(S2)
            new_lines.extend(H3)
            
            new_lines.extend(I1)
            new_lines.extend(S1)
            new_lines.extend(I2)
            new_lines.extend(S2)
            new_lines.extend(I3)
            
            i = end_i + 1
        else:
            # For C4 and C5 (lists of items in m_Modifications)
            head = []
            incoming = []
            i += 1
            while not lines[i].startswith('======='):
                head.append(lines[i])
                i += 1
            i += 1
            while not lines[i].startswith('>>>>>>>'):
                incoming.append(lines[i])
                i += 1
            i += 1
            new_lines.extend(head)
            new_lines.extend(incoming)
    else:
        new_lines.append(lines[i])
        i += 1

with open(file_path, 'w', encoding='utf-8') as f:
    f.writelines(new_lines)

print("Merge conflicts resolved successfully.")
