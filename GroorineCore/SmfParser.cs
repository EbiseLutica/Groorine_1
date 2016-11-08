using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using static GroorineCore.MidiTimingConverter;

namespace GroorineCore
{
	/// <summary>
	/// Standard MIDI File を Groorine プロジェクト形式としてインポートする機能を提供します。このクラスは継承できません。
	/// </summary>
	public static class SmfParser
	{
		public static GroorineFile Parse(Stream data)
		{
			var tracks = new ObservableCollection<Track>();
			var title = "";
			var copyright = "";
			long? loopStart = null;
			var metas = new ObservableCollection<MetaEvent>();
			
			using (var br = new BinaryReader(data, Encoding.UTF8))
			{
				
				// Magic Number MThd
				if (new string(br.ReadChars(4)) != "MThd")
				{
					throw new ArgumentException("SMF ファイルではないファイルを読み込もうとしました");
				}

				var chunklen = br.ReadInt32BE();
				var format = br.ReadInt16BE();
				var trackNum = br.ReadInt16BE();
				var resolution = br.ReadInt16BE();
				
				//Debug.WriteLine($"MThd: {chunklen} {format} {trackNum} {resolution}");
				for (var i = 0; i < trackNum; i++)
				{
					//Debug.WriteLine($"{i} / {trackNum}");
					br.ReadBytes(4);
					//Debug.WriteLine("MTrk");
					var size = br.ReadInt32BE();
					//Debug.WriteLine($"Size: {size}");
					var events = new ObservableCollection<MidiEvent>();
					Track mt;
					tracks.Add(mt = new Track(events));
					var noteDic = new Dictionary<byte, NoteEvent>();
					var btype = 0;
					var tick = 0;
					var j = 0;
					while (j < size)
					{
						//Debug.WriteLine($"{j} / {size}");
						var length = br.ReadVariableLength(ref j);
						//Debug.WriteLine($"DeltaTime: {length}");
						tick += length;
						var eventStatus = br.ReadByte();
						j++;
						//Debug.WriteLine($"status: {eventStatus}");
						switch (eventStatus)
						{
							case 0xFF:
								// MetaEvent
								var type = br.ReadByte();
								var len = br.ReadVariableLength(ref j);
								byte[] d = br.ReadBytes(len);
								char[] moji = (from el in d
									select (char) el).ToArray();
								
								//Debug.WriteLine($"MetaEvent {type:x} {len:x} {new string(moji)}");
								j++;
								j += len;
								switch (type)
								{
									case 0x02:
										copyright = new string(moji);
										break;
									case 0x03:
										mt.Name = new string(moji);
										if (i == 0)
											title = new string(moji);
										break;
									case 0x51:
										metas.Add(new TempoEvent
										{
											Tempo = TempoToBpm(d[0] << 16 | d[1] << 8 | d[2]),
											Tick = tick
										});
										break;
									case 0x58:
										int Pow(int a, int b)
										{
											for (var hage = 1; hage < b; hage++)
												a *= a;
											return a;
										}
										metas.Add(new BeatEvent
										{
											Rhythm = d[0],
											Note = Pow(2, d[1])
										});
										break;
								}
								break;
							case 0xF7:
								// SysEx
								len = br.ReadVariableLength(ref j);
								d = br.ReadBytes(len);
								j += len;
								break;
							case 0xF0:
								// SysEx
								len = br.ReadVariableLength(ref j);
								d = br.ReadBytes(len);
								br.ReadByte();
								j += len + 1;
								break;
							default:
								// Other
								type = eventStatus;
								var channel = (byte)(type & 0xf);
								//Debug.WriteLine($"{channel:x} {type:x}");
								switch (type & 0xF0)
								{
									case 0x90:
										// Note On
										var note = br.ReadByte();
										var vel = br.ReadByte();
										j += 2;
										/*events.Add(new NoteOnEvent
										{
											Channel = channel,
											Note = note,
											Velocity = vel,
											Tick = tick
										});*/

										if (noteDic.ContainsKey(note))
										{
											noteDic[note].Gate = tick - noteDic[note].Tick;
											noteDic.Remove(note);
										}
										if (vel > 0)
										{
											events.Add(noteDic[note] = new NoteEvent
											{
												Channel = channel,
												Note = note,
												Velocity = vel,
												Tick = tick
											});
										}
										break;
									case 0x80:
										// Note Off
										note = br.ReadByte();
										vel = br.ReadByte();
										j += 2;
										if (noteDic.ContainsKey(note))
										{
											noteDic[note].Gate = tick - noteDic[note].Tick;
											noteDic.Remove(note);
										}
										break;
									case 0xB0:
										// Control Change
										byte no = br.ReadByte(), dat = br.ReadByte();
										if (no == 111)
										{
											loopStart = tick;
											j += 2;
											break;
										}
										events.Add(new ControlEvent
										{
											Channel = channel,
											Tick = tick,
											ControlNo = no,
											Data = dat
										});
										j += 2;
										break;
									case 0xC0:
										// Program Change
										events.Add(new ProgramEvent
										{
											Channel = channel,
											Tick = tick,
											ProgramNo = br.ReadByte()
										});
										j++;
										break;
									case 0xE0:
										// Pitch Bend
										var l = br.ReadByte();
										var m = br.ReadByte();
										j += 2;
										events.Add(new PitchEvent
										{
											Channel = channel,
											Tick = tick,
											Bend = (short)((m << 7 | l) - 8192)
										});
										break;
									case 0xA0:
										// PAT
										l = br.ReadByte();
										m = br.ReadByte();
										events.Add(new PolyphonicKeyPressureEvent
										{
											Channel = channel,
											Tick = tick,
											NoteNumber = l,
											Pressure = m
										});
										break;
									case 0xD0:
										// CAT
										l = br.ReadByte();
										events.Add(new ChannelPressureEvent
										{
											Channel = channel,
											Tick = tick,
											Pressure = l
										});
										break;
									default:
										if ((type & 0x80) == 0)
										{
											// ランニングステータス
											switch (btype)
											{
												case 0x80:
													// Note On
													vel = br.ReadByte();
													if (noteDic.ContainsKey(type))
													{
														noteDic[type].Gate = tick - noteDic[type].Tick;
														noteDic.Remove(type);
													}
													if (vel > 0)
													{
														events.Add(noteDic[type] = new NoteEvent
														{
															Channel = channel,
															Note = type,
															Velocity = vel,
															Tick = tick
														});
													}
													j++;
													break;
												case 0xB0:
													// Control Change
													events.Add(new ControlEvent
													{
														Channel = channel,
														Tick = tick,
														ControlNo = type,
														Data = br.ReadByte()
													});
													j++;
													break;
												case 0xC0:
													// Program Change
													events.Add(new ProgramEvent
													{
														Channel = channel,
														Tick = tick,
														ProgramNo = type
													});
													break;
												case 0xE0:
													// Pitch Bend:
													events.Add(new PitchEvent
													{
														Channel = channel,
														Tick = tick,
														Bend = (short)((br.ReadByte() << 7 | type - 8192))
													});
													j++;
													break;
												case 0xA0:
													// PAT
													events.Add(new PolyphonicKeyPressureEvent
													{
														Channel = channel,
														Tick = tick,
														NoteNumber = type,
														Pressure = br.ReadByte()
													});
													break;
												case 0xD0:
													// CAT
													events.Add(new ChannelPressureEvent
													{
														Channel = channel,
														Tick = tick,
														Pressure = type
													});
													break;
											}
										}
										break;

								}
								btype = type;
								break;
						}
					}
				}
				return new GroorineFile(new ConductorTrack(metas, resolution), tracks, resolution, title, copyright);
			}

			//塩基[] 塩基配列 = {A, G, C, T, A, A, T, G, G, T, C, A, C, C, A, G, G, T};

			
		}

		

	}
}
