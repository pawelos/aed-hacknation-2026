import { test } from 'node:test'
import assert from 'node:assert/strict'
import { describeOpeningHours } from './openingHours.ts'

const pl = { always: 'całą dobę', holidays: 'święta', closed: 'nieczynne' }
const en = { always: '24/7', holidays: 'public holidays', closed: 'closed' }

test('days, times and holidays in Polish', () => {
    assert.equal(
        describeOpeningHours(
            'Mo-Fr 08:00-16:00; Sa 10:00-14:00; PH off',
            'pl',
            pl,
        ),
        'pon.–pt. 08:00–16:00; sob. 10:00–14:00; święta nieczynne',
    )
})

test('months in Polish', () => {
    assert.equal(
        describeOpeningHours('Jun-Aug 08:00-22:00', 'pl', pl),
        'cze–sie 08:00–22:00',
    )
})

test('24/7 and an empty value', () => {
    assert.equal(describeOpeningHours('24/7', 'pl', pl), 'całą dobę')
    assert.equal(describeOpeningHours('  ', 'pl', pl), '')
})

test('English keeps the usual names', () => {
    assert.equal(
        describeOpeningHours('Mo-Fr 08:00-16:00', 'en', en),
        'Mon–Fri 08:00–16:00',
    )
})
