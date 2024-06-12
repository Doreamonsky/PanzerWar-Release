import os
import json
import hashlib
import argparse  # Import the argparse module

def split_file(file_path, chunk_size=104857600, base_url="http://yourserver.com/path/to/files/"):
    file_size = os.path.getsize(file_path)
    part_num = 0
    parts_info = []

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
                break

            part_num += 1
            part_name = f"{os.path.splitext(os.path.basename(file_path))[0].lower()}.part{part_num}"
            part_path = os.path.join(os.path.dirname(file_path), part_name)

            with open(part_path, 'wb') as part:
                part.write(chunk)
                part_size = os.path.getsize(part_path)  

            parts_info.append({
                "part_number": part_num,
                "file_name": part_name,
                "url": base_url + part_name,
                "size": part_size,
                "file_md5" : md5_of_file(part_path)  
            })

    output_info = {
        "file_name": os.path.basename(file_path).lower(),
        "file_md5": file_md5,
        "parts": parts_info
    }

    json_file_path = os.path.join(os.path.dirname(file_path), "apk_ship.json")
    with open(json_file_path, 'w') as json_file:
        json.dump(output_info, json_file, indent=4)
    
    return f"Finished splitting {file_path} into {part_num} parts. Parts information saved to {json_file_path}", output_info

def split_all_apks_in_directory(directory_path, chunk_size=52428800, base_url="http://yourserver.com/path/to/files/"):
    for root, dirs, files in os.walk(directory_path):
        for file in files:
            if file.endswith(".Apk") or file.endswith(".apk"):
                file_path = os.path.join(root, file)
                print(split_file(file_path, chunk_size, base_url))

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description='Split APK files into smaller chunks')
    parser.add_argument('-d', '--directory', type=str, required=True, help='Directory containing APK files to split')
    parser.add_argument('-u', '--url', type=str, required=True, help='The remote url for downloading')
    args = parser.parse_args()

    directory_path = args.directory  # Use the directory path from the command line argument
    base_url = args.url
    split_all_apks_in_directory(directory_path, 50 * 1024 * 1024, base_url)