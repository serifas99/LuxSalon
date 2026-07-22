// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'notifikacija.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

Notifikacija _$NotifikacijaFromJson(Map<String, dynamic> json) =>
    Notifikacija(
      id: (json['id'] as num?)?.toInt(),
      korisnikId: (json['korisnikId'] as num?)?.toInt(),
      naslov: json['naslov'] as String?,
      poruka: json['poruka'] as String?,
      tip: json['tip'] as String?,
      procitano: json['procitano'] as bool?,
      createdAt: json['createdAt'] == null
          ? null
          : DateTime.parse(json['createdAt'] as String),
      terminId: (json['terminId'] as num?)?.toInt(),
    );

Map<String, dynamic> _$NotifikacijaToJson(Notifikacija instance) =>
    <String, dynamic>{
      'id': instance.id,
      'korisnikId': instance.korisnikId,
      'naslov': instance.naslov,
      'poruka': instance.poruka,
      'tip': instance.tip,
      'procitano': instance.procitano,
      'createdAt': instance.createdAt?.toIso8601String(),
      'terminId': instance.terminId,
    };
