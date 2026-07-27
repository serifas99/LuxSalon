// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'radno_vrijeme.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

RadnoVrijeme _$RadnoVrijemeFromJson(Map<String, dynamic> json) =>
    RadnoVrijeme(
      id: (json['id'] as num?)?.toInt(),
      frizerId: (json['frizerId'] as num?)?.toInt(),
      frizerImePrezime: json['frizerImePrezime'] as String?,
      danUSedmici: (json['danUSedmici'] as num?)?.toInt(),
      danUSedmiceNaziv: json['danUSedmiceNaziv'] as String?,
      pocetakRada: json['pocetakRada'] as String?,
      krajRada: json['krajRada'] as String?,
      neRadi: json['neRadi'] as bool?,
    );

Map<String, dynamic> _$RadnoVrijemeToJson(RadnoVrijeme instance) =>
    <String, dynamic>{
      'id': instance.id,
      'frizerId': instance.frizerId,
      'frizerImePrezime': instance.frizerImePrezime,
      'danUSedmici': instance.danUSedmici,
      'danUSedmiceNaziv': instance.danUSedmiceNaziv,
      'pocetakRada': instance.pocetakRada,
      'krajRada': instance.krajRada,
      'neRadi': instance.neRadi,
    };
