﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SongUploadAPI.Contracts.Requests
{
    public class SongUpdateRequest
    {
        [Required]
        public string Name { get;}
        [Required]
        public string Artist { get;}
        [Required]
        public string Key { get;}
        [Required]
        public int Bpm { get;}

        public SongUpdateRequest(string name, string artist, string key, int bpm)
        {
            Name = name;
            Artist = artist;
            Key = key;
            Bpm = bpm;
        }
    }
    
}
