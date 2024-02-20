import os
import json
import hashlib

def split_file(file_path, chunk_size=104857600, base_url="http://yourserver.com/path/to/files/"):
    file_size = os.path.getsize(file_path)
    part_num = 0
    parts_info = []

    # 计算文件的MD5
    def md5_of_file(file_path):
        hash_md5 = hashlib.md5()
        with open(file_path, "rb") as f:
            for chunk in iter(lambda: f.read(4096), b""):
                hash_md5.update(chunk)
        return hash_md5.hexdigest()

    file_md5 = md5_of_file(file_path)

    with open(file_path, 'rb') as f:
        while True:
            chunk = f.read(chunk_size)
            if not chunk:
                break  # 文件结束

            part_num += 1
            part_name = f"{os.path.basename(file_path)}.part{part_num}"
            part_path = os.path.join(os.path.dirname(file_path), part_name)

            with open(part_path, 'wb') as part:
                part.write(chunk)
                part_size = os.path.getsize(part_path)  

            parts_info.append({
                "part_number": part_num,
                "file_name": part_name,
                "url": base_url + part_name,
                "size": part_size  
            })

    # 将文件的MD5和名称加入到JSON中
    output_info = {
        "file_name": os.path.basename(file_path),
        "file_md5": file_md5,
        "parts": parts_info
    }

    json_file_path = os.path.join(os.path.dirname(file_path),"apk_ship.json")
    with open(json_file_path, 'w') as json_file:
        json.dump(output_info, json_file, indent=4)
    
    return f"Finished splitting {file_path} into {part_num} parts. Parts information saved to {json_file_path}", output_info


if __name__ == "__main__":
    file_path = "PanzerWar-v2024.2.18.6-OBT-Global.apk" 
    base_url = "https://dl.windyverse.net/apk/" 
    text = split_file(file_path, 100 * 1024 * 1024, base_url)  
    print(text)
