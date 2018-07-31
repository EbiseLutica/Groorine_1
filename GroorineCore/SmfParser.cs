using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Groorine.DataModel;
using Groorine.Events;
using Groorine.Helpers;
using static Groorine.Helpers.MidiTimingConverter;

namespace Groorine
{
	/// <summary>
	/// Standard MIDI File を Groorine プロジェクト形式としてインポートする機能を提供します。このクラスは継承できません。
	/// </summary>
	public static class SmfParser
	{
		static int Pow(int a, int b)
		{
			var r = a;
			for (var hage = 1; hage < b; hage++)
				r *= a;
			return r;
		}

		/// <summary>
		/// スタンダード MIDI ファイルを読み込み解析します。
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static MidiFile Parse(Stream data)
		{
			var tracks = new ObservableCollection<Track>();
			var title = "";
			var copyright = "";
			long? loopStart = null;
			var metas = new ObservableCollection<MetaEvent>();

			using (var br = new BinaryReader(data, Encoding.UTF8))
			{

				// Magic Number MThd
				if (br.ReadString(4) != "MThd")
				{
					throw new ArgumentException("SMF ファイルではないファイルを読み込もうとしました");
				}

				var chunklen = br.ReadInt32BE();
				var format = br.ReadInt16BE();
				var trackNum = br.ReadInt16BE();
				var resolution = br.ReadInt16BE();

				for (var i = 0; i < trackNum; i++)
				{
					br.ReadBytes(4);
					var size = br.ReadInt32BE();
					var events = new ObservableCollection<MidiEvent>();
					Track mt;
					tracks.Add(mt = new Track(events));
					mt.Name = $"Track {i + 1}";
					var noteDic = new Dictionary<byte, NoteEvent>();
					var prevType = 0;
					byte prevChannel = 0;
					var tick = 0;
					var j = 0;
					while (j < size)
					{
						var length = br.ReadVariableLength(ref j);
						tick += length;
						var eventStatus = br.ReadByte();
						j++;
						switch (eventStatus)
						{
							case 0xFF:
								// MetaEvent
								var type = br.ReadByte();
								var len = br.ReadVariableLength(ref j);
								byte[] d = br.ReadBytes(len);
								var moji = Encoding.UTF8.GetString(d, 0, len);

								j++;
								j += len;
								switch (type)
								{
									case 0x01:
										events.Add(new CommentEvent(moji)
										{
											Tick = tick
										});
										break;
									case 0x02:
										copyright = moji;
										break;
									case 0x03:
										if (moji != "")
										{
											mt.Name = moji;
										}
										if (i == 0)
											title = moji;
										break;
									case 0x05:
										events.Add(new LyricsEvent(moji)
										{
											Tick = tick
										});
										break;
									case 0x2F:
										events.Add(new EndOfTrackEvent
										{
											Tick = tick
										});
										break;
									case 0x51:
										metas.Add(new TempoEvent
										{
											Tempo = TempoToBpm(d[0] << 16 | d[1] << 8 | d[2]),
											Tick = tick
										});
										break;
									case 0x58:
										metas.Add(new BeatEvent
										{
											Tick = tick,
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
								events.Add(new SysExEvent
								{
									Data = d,
									Tick = tick
								});
								break;
							case 0xF0:
								// SysEx
								len = br.ReadVariableLength(ref j) - 1;
								d = br.ReadBytes(len);
								br.ReadByte();
								j += len + 1;

								events.Add(new SysExEvent
								{
									Data = d,
									Tick = tick
								});
								break;
							default:
								// Other
								type = eventStatus;
								var channel = (byte)(type & 0xf);
								switch (type & 0xF0)
								{
									case 0x90:
										// Note On
										var note = br.ReadByte();
										var vel = br.ReadByte();
										j += 2;
										if (vel == 0 && noteDic.ContainsKey(note))
										{
											noteDic[note].Gate = tick - noteDic[note].Tick;
											events.Add(noteDic[note]);
											noteDic.Remove(note);
										}
										if (vel > 0)
										{
											noteDic[note] = new NoteEvent
											{
												Channel = channel,
												Note = note,
												Velocity = vel,
												Tick = tick
											};
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
											events.Add(noteDic[note]);
											noteDic.Remove(note);
										}
										break;
									case 0xB0:
										// Control Change
										byte no = br.ReadByte(), dat = br.ReadByte();
										j += 2;
										if (no == 111)
										{
											loopStart = tick;
											break;
										}
										events.Add(new ControlEvent
										{
											Channel = channel,
											Tick = tick,
											ControlNo = no,
											Data = dat
										});
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
										j += 2;
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
										events.Add(new ChannelPressureEvent
										{
											Channel = channel,
											Tick = tick,
											Pressure = br.ReadByte()
										});
										j++;
										break;
									default:
										if ((type & 0x80) == 0)
										{
											// ランニングステータス
											switch (prevType & 0xF0)
											{
												case 0x90:
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
															Channel = prevChannel,
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
														Channel = prevChannel,
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
														Channel = prevChannel,
														Tick = tick,
														ProgramNo = type
													});
													break;
												case 0xE0:
													// Pitch Bend:
													events.Add(new PitchEvent
													{
														Channel = prevChannel,
														Tick = tick,
														Bend = (short)((br.ReadByte() << 7 | type - 8192))
													});
													j++;
													break;
												case 0xA0:
													// PAT
													events.Add(new PolyphonicKeyPressureEvent
													{
														Channel = prevChannel,
														Tick = tick,
														NoteNumber = type,
														Pressure = br.ReadByte()
													});
													j++;
													break;
												case 0xD0:
													// CAT
													events.Add(new ChannelPressureEvent
													{
														Channel = prevChannel,
														Tick = tick,
														Pressure = type
													});
													break;
											}
										}
										break;
								
								}
								if ((type & 0x80) != 0)
								{
									prevType = type;
									prevChannel = channel;
								}
								break;
						}
					}
				}
				return new MidiFile(new ConductorTrack(metas, resolution), tracks, resolution, title, copyright, loopStart);
			}
		}

		/// <summary>
		/// Format 1 のスタンダード MIDI ファイル形式で書き出します。
		/// </summary>
		/// <param name="output"></param>
		/// <param name="mf"></param>
		public static void Save(Stream output, MidiFile mf)
		{
			using (var writer = new BinaryWriter(output))
			{

				// header
				writer.WriteString("MThd");

				// data length
				writer.WriteBE(6);

				// format version
				writer.WriteBE((short)1);

				// track count
				writer.WriteBE((short)mf.Tracks.Count);

				// resolution
				writer.WriteBE(mf.Resolution);

				// 指揮者トラックと楽曲トラックに含まれる全データを処理する
				foreach (IEnumerable<MidiEvent> track in new List<IEnumerable<MidiEvent>> { mf.Conductor.Events.Cast<MidiEvent>() }.Concat(mf.Tracks.Select(t => t.Events)))
				{
					// トラックごとにメモリ上に一旦書き出す(チャンクの長さを取るため)
					var ms = new MemoryStream();

					using (var trackWriter = new BinaryWriter(ms))
					{
						void WriteAll(params byte[] bytes)
						{
							trackWriter.Write(bytes);
						}
						

						foreach (MidiEvent e in track.OrderBy(e => e.Tick))
						{
							int prevTick = 0;
							int tick = (int)e.Tick;

							// デルタタイム
							// 前回のtickとの差分を取る。和音なら同じtickなので0になる
							trackWriter.WriteVariableLength(tick - prevTick);
							
							switch (e)
							{
								case BeatEvent b:
									WriteAll(0xff, 0x58, 4, (byte)b.Rhythm, (byte)Math.Log(b.Note, 2), 0x18, 0x8);
									break;
								case ChannelPressureEvent cp:
									WriteAll((byte)(0xD0 + cp.Channel), cp.Pressure);
									break;
								case CommentEvent cmt:
									WriteAll(0xFF, 0x01);
									trackWriter.WriteVariableLength(cmt.Text.Length);
									trackWriter.WriteString(cmt.Text);
									break;
								case ControlEvent cc:
									WriteAll((byte)(0xB0 + cc.Channel), cc.ControlNo, cc.Data);
									break;
								case EndOfTrackEvent eot:
									WriteAll(0xff, 0x2f, 0x00);
									break;
								case LyricsEvent ly:
									WriteAll(0xFF, 0x05);
									trackWriter.WriteVariableLength(ly.Text.Length);
									trackWriter.WriteString(ly.Text);
									break;
								case NoteEvent n:
									//note on
									WriteAll((byte)(0x90 + n.Channel), n.Note, n.Velocity);

									//note off
									trackWriter.WriteVariableLength((int)n.Gate);
									WriteAll((byte)(0x90 + n.Channel), n.Note, 0);

									tick += (int)n.Gate;
									break;
								case PitchEvent pb:
									var msb = (byte)(((pb.Bend + 8192) >> 8) & 0x7f);
									var lsb = (byte)((pb.Bend + 8192) & 0x7f);
									WriteAll((byte)(0xE0 + pb.Channel), msb, lsb);
									break;
								case PolyphonicKeyPressureEvent pkp:
									WriteAll((byte)(0xA0 + pkp.Channel), pkp.NoteNumber, pkp.Pressure);
									break;
								case ProgramEvent pc:
									WriteAll((byte)(0xC0 + pc.Channel), pc.ProgramNo); 
									break;
								case SysExEvent sysex:
									trackWriter.Write((byte)0xf0);
									trackWriter.WriteVariableLength(sysex.Data.Length);
									trackWriter.Write(sysex.Data);
									trackWriter.Write((byte)0xf7);
									break;
								case TempoEvent tempo:
									var t = Math.Min(1677215, 60000000 / tempo.Tempo);
									WriteAll(0xff, 0x51, 0x03);
									trackWriter.Write(BitConverter.GetBytes(t).Take(3).Reverse().ToArray());
									break;
							}
							prevTick = tick;
						}

						// 実際に書き込む

						// マジックナンバー
						writer.WriteString("MTrk");
						// 長さ
						writer.WriteBE((int)ms.Length);
						// データを書いて終わり
						ms.WriteTo(output);
					}
				}
				writer.Flush();
			}

		}
	}
}