db.createCollection("styles", {
	validator: {
		$jsonSchema: {
			bsonType: "object",
			required: [ "name" ],
			properties: {
				name: {
				   bsonType: "string",
				   description: "must be a string and is required"
				}
			}
		}
	}
})

db.styles.insertOne({name: "Classic"})
db.styles.insertOne({name: "Rock"})
db.styles.insertOne({name: "Jazz"})

db.createCollection("bands", {
   validator: {
		$jsonSchema: {
			bsonType: "object",
			required: [ "name", "style" ],
			properties: {
				name: {
				   bsonType: "string",
				   description: "must be a string and is required"
				},
				style: {
					bsonType: "object",
					required: ["_id", "name"],
					properties:{
						_id: {
							bsonType: "objectId",
							description: "must be a string and is required"
							},
						name: {
							bsonType: "string",
							description: "must be a string and is required"
							}
					}
				}
			}
		}
	}
})

db.createCollection("song", {
    validator: {
        $jsonSchema: {
            bsonType: "object",
            required: ["name", "style", "band"],
            properties: {
                name: {
                    bsonType: "string",
                    description: "must be a string and is required"
                },
                style: {
                    bsonType: "object",
                    required: ["_id", "name"],
                    properties: {
                        _id: {
                            bsonType: "objectId",
                            description: "must be a string and is required"
                        },
                        name: {
                            bsonType: "string",
                            description: "must be a string and is required"
                        }
                    }
                },
                band: {
                    bsonType: "object",
                    required: ["_id", "name"],
                    properties: {
                        _id: {
                            bsonType: "objectId",
                            description: "must be a string and is required"
                        },
                        name: {
                            bsonType: "string",
                            description: "must be a string and is required"
                        }
                    }
                },
                midiBase64Encoded: {
                    bsonType: "string",
                    description: "must be a string and is required"
                },
                durationInSeconds: {
                    bsonType: "int",
                    description: "must be an integer"
                },
                durationInTicks: {
                    bsonType: "long",
                    description: "must be an integer"
                },
                averageTempo: {
                    bsonType: "int",
                    description: "must be an integer"
                },
                bars: {
                    bsonType: "array",
                    uniqueItems: true,
                    additionalProperties: false,
                    items: {
                        bsonType: "object",
                        required: ["barNumber"],
                        properties: {
                            barNumber: {
                                bsonType: "int",
                                description: "must be an integer"
                            },
                            ticksFromBeginningOfSong: {
                                bsonType: "int",
                                description: "must be an integer"
                            },
                            tempoInMicrosecondsPerQuarterNote: {
                                bsonType: "int",
                                description: "must be an integer"
                            },
                            hasTriplets: {
                                bsonType: "bool",
                                description: "must be a bool"
                            },
                            timeSignature: {
                                bsonType: "object",
                                required: ["numerator", "denominator"],
                                properties: {
                                    numerator: {
                                        bsonType: "int",
                                        description: "must be an integer"

                                    },
                                    denominator: {
                                        bsonType: "int",
                                        description: "must be an integer"

                                    }
                                }
                            },
                            keySignature: {
                                bsonType: "object",
                                required: ["key", "scaleType"],
                                properties: {
                                    key: {
                                        bsonType: "int",
                                        description: "must be an integer"

                                    },
                                    scale: {
                                        bsonType: "string",
                                        description: "must be a string"

                                    }
                                }
                            }
                        }
                    }
                },
                tempoChanges: {
                    bsonType: "array",
                    items: {
                        bsonType: "object",
                        required: ["microsecondsPerQuarterNote", "ticksSinceBeginningOfSong"],
                        properties: {
                            microsecondsPerQuarterNote: {
                                bsonType: "long",
                                description: "must be an integer"
                            },
                            ticksSinceBeginningOfSong: {
                                bsonType: "long",
                                description: "must be an integer"
                            }
                        }
                    }
                },
                simplifications: {
                    bsonType: "array",
                    items: {
                        bsonType: "object",
                        properties: {
                            version: {
                                bsonType: "int",
                                description: "must be an integer"
                            },
                            numberOfVoices: {
                                bsonType: "int",
                                description: "must be an integer"
                            },
                            notes: {
                                bsonType: "array",
                                items: {
                                    bsonType: "object",
                                    required: ["pitch", "volume", "startSinceBeginningOfSongInTicks", "endSinceBeginningOfSongInTicks", "voice"],
                                    properties: {
                                        guid: {
                                            bsonType: "string",
                                            description: "must be a string"
                                        },
                                        pitch: {
                                            bsonType: "int",
                                            description: "must be an integer"
                                        },
                                        volume: {
                                            bsonType: "int",
                                            description: "must be an integer"
                                        },
                                        startSinceBeginningOfSongInTicks: {
                                            bsonType: "long",
                                            description: "must be an integer"
                                        },
                                        endSinceBeginningOfSongInTicks: {
                                            bsonType: "long",
                                            description: "must be an integer"
                                        },
                                        voice: {
                                            bsonType: "int",
                                            description: "must be an integer"
                                        },
                                        instrument: {
                                            bsonType: "int",
                                            description: "must be an integer"
                                        },
                                        isPercussion: {
                                            bsonType: "bool",
                                            description: "must be a boolean"
                                        },
                                        pitchBending: {
                                            bsonType: "array",
                                            items: {
                                                bsonType: "object",
                                                properties: {
                                                    ticksSinceBeginningOfSong: {
                                                        bsonType: "long",
                                                        description: "must be a long"
                                                    },
                                                    pitch: {
                                                        bsonType: "int",
                                                        description: "must be an int"
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                midiStats: {
                    bsonType: "object",
                    required: [],
                    properties: {
                        totalTracks: {
                            bsonType: "int",
                            description: "must be an integer"
                        },
                        totalChannels: {
                            bsonType: "int",
                            description: "must be an integer"
                        },
                        totalEvents: {
                            bsonType: "int",
                            description: "must be an integer"
                        },
                        totalNoteEvents: {
                            bsonType: "int",
                            description: "must be an integer"
                        },
                        totalTempoChanges: {
                            bsonType: "int",
                            description: "must be an integer"
                        },
                        totalPitchBendEvents: {
                            bsonType: "int",
                            description: "must be an integer"
                        },
                        totalProgramChangeEvents: {
                            bsonType: "int",
                            description: "must be an integer"
                        },
                        totalControlChangeEvents: {
                            bsonType: "int",
                            description: "must be an integer"
                        },
                        totalSustainPedalEvents: {
                            bsonType: "int",
                            description: "must be an integer"
                        },
                        totalChannelIndependentEvents: {
                            bsonType: "int",
                            description: "must be an integer"
                        },
                    }
                }
            }
        }
    }
})




let stylito = db.styles.findOne({ name: "Classic" })

db.bands.insertOne({ name: "John Sebastian Bach", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Wolfgang Amadeus Mozart", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Ludwig van Beethoven", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Frederic Chopin", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Antonio Vivaldi", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "George Frideric Handel", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Joseph Haydn", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Franz Schubert", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Franz Liszt", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Johannes Brahms", style: { _id: stylito._id, name: stylito.name } })


stylito = db.styles.findOne({ name: "Rock" })

db.bands.insertOne({ name: "AC DC", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Al Stewart", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Alan Parsons", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Alice Cooper", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "America", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Beatles", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Bee Gees", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Black Sabbath", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Boston", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Creedence Clearwater Revival", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Crosby Stills Nash", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Deep Purple", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Def Leppard", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Dire Straits", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Doors", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Duran Duran", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Eagles", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Emerson Lake Palmer", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Elton John", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Genesis", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Guns and Roses", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Jimmy Hendrix", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Jethro Tull", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Kansas", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Led Zeppelin", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Michael Jackson", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Midnight Oil", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Nirvana", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Oasis", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Pet Shop Boys", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Peter Gabriel", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Pink Floyd", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Police", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Queen", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "REM", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Rolling Stones", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Roxette", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Rush", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Santana", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Scorpions", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Styx", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Supertramp", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Toto", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "U2", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Van Halen", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Van Whitesnake", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Yes", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "ZZ Top", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Alphaville", style: { _id: stylito._id, name: stylito.name } })
db.bands.insertOne({ name: "Aha", style: { _id: stylito._id, name: stylito.name } })

let bandita = db.bands.findOne({ name: "Van Halen" })

db.songs.insertOne({
    name: "Eruption",
    style: { _id: stylito._id, name: stylito.name },
    band: { _id: bandita._id, name: bandita.name, style: { _id: stylito._id, name: stylito.name } },
    tempoChanges: [
        { microsecondsPerQuarterNote: 12345678, ticksSinceBeginningOfSong: 0 },
        { microsecondsPerQuarterNote: 34567890, ticksSinceBeginningOfSong: 220}],
    bars: [
        {
            barNumber: 1,
            ticksFromBeginningOfSong: 0,
            timeSignature: {
                numerator: 4,
                denominator: 4
            },
            keySignature: {
                key: 3,
                scale: "major"
            }
        }, {
            barNumber: 2,
            ticksFromBeginningOfSong: 384,
            timeSignature: {
                numerator: 4,
                denominator: 4
            },
            keySignature: {
                key: 3,
                scale: "major"
            }
        }]
})