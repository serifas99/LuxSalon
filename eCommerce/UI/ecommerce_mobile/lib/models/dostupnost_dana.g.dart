// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'dostupnost_dana.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

DostupnostDana _$DostupnostDanaFromJson(Map<String, dynamic> json) =>
    DostupnostDana(
      datum: DateTime.parse(json['datum'] as String),
      radi: json['radi'] as bool,
      slobodno: json['slobodno'] as bool,
      brojSlobodnihSlotova: (json['brojSlobodnihSlotova'] as num).toInt(),
    );

Map<String, dynamic> _$DostupnostDanaToJson(DostupnostDana instance) =>
    <String, dynamic>{
      'datum': instance.datum.toIso8601String(),
      'radi': instance.radi,
      'slobodno': instance.slobodno,
      'brojSlobodnihSlotova': instance.brojSlobodnihSlotova,
    };
